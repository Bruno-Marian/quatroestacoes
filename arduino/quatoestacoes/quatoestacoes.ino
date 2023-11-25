
#include <ArduinoOTA.h>
#include <PubSubClient.h>
#include <WiFiManager.h>
#include <Arduino.h>
#include <Adafruit_AHT10.h>
#include <ArduinoJson.h>

// Sensor temperatura e humidade
Adafruit_AHT10 aht;
String temp1 = "";
String umid1 = "";

// Sensor de luminosidade
int pino_a36 = 36;
int val_pin36 = 0;

// Sensor de chuva
int pin_a15 = 39;
int val_pin15 = 0;

//__ Informações do dispositivo
//__ Variáveis de conexão com o servidor
String DEVICE_ID;
const String server = "blu.sisgel.com";
String topicoChuva = "quatroestacoes/chuva/";
String topicoHumidade = "quatroestacoes/humidade/";
String topicoTemperatura = "quatroestacoes/temperatura/";
String topicoLuminosidade = "quatroestacoes/luminosidade/";
String topicoStart = "quatroestacoes/start/";
char authMeth[] = "Senac";
char token[] = "Senac";
String clientId;
DynamicJsonDocument doc(1024);
WiFiClient wifiClient;
PubSubClient client(server.c_str(), 41883, wifiClient);


//Função responsável pela conexão ao servidor MQTT
void connectMQTTServer() {
  Serial.println("Conectando ao servidor MQTT...");

  if (client.connect(clientId.c_str(), authMeth, token)) {
    Serial.println("Conectado ao Broker MQTT...");
    client.setCallback(callback);

  } else {
    Serial.print("erro = ");
    Serial.println(client.state());
    connectMQTTServer();
  }
}

void callback(char* topic, unsigned char* payload, unsigned int length) {
  Serial.print("topico ");
  Serial.println(topic);
  String sPayload = "";
  for (int i = 0; i < length; i++) { sPayload += (char)payload[i]; }
  Serial.println(sPayload);
}

void configModeCallback(WiFiManager* myWiFiManager) {
  Serial.println("Entrou no modo de configuração");
  Serial.println(WiFi.softAPIP());
  Serial.println(myWiFiManager->getConfigPortalSSID());
}

void saveConfigCallback() {
  Serial.println("Configuração salva");
  Serial.println(WiFi.softAPIP());
}

void atualizaDados() {
  static unsigned long delay1 = 0;
  int tempo = 5000;

  if ((millis() - delay1) > tempo) {
    atualizaEnviaLuminosidade();
    atualizaEnviaChuva();
    atualizaEnviaTemperaturaHumidade();
  }
  if ((millis() - delay1) >= (tempo + 100)) {
    delay1 = millis();
  }
}

void atualizaEnviaStart() {
  doc.clear();
  DEVICE_ID = String(WiFi.macAddress());
  clientId = DEVICE_ID;
  doc["device_id"] = DEVICE_ID;
  doc["start"] = "ZFEE";
  doc["mac"] = DEVICE_ID;
  doc["wifiSSID"] = String(WiFi.SSID());

  enviarDados(doc, topicoStart);
}

void atualizaEnviaLuminosidade() {
  val_pin36 = analogRead(pino_a36);
  Serial.print("Luminosidade: ");
  Serial.println(val_pin36);
  if (val_pin36 >= 0){
    doc.clear();
    doc["luminosidade"] = String(val_pin36);
    enviarDados(doc, topicoLuminosidade);
  }
}

void atualizaEnviaChuva(){
  val_pin15 = analogRead(pin_a15);
  Serial.print("Chuva: ");
  Serial.println(val_pin15);
  if (val_pin15 > 0){
    doc.clear();
    doc["chuva"] = String(val_pin15);
    enviarDados(doc, topicoChuva);
  }
}

void atualizaEnviaTemperaturaHumidade() {
  sensors_event_t humidity, temp;
  aht.getEvent(&humidity, &temp);
  
  Serial.print("Temperatura:");
  Serial.println(temp.temperature);

  if (temp.temperature > 0) {
    doc.clear();
    doc["temperatura"] = String(temp.temperature);
    enviarDados(doc, topicoTemperatura);
  }

  Serial.print("Humidade:");
  Serial.println(humidity.relative_humidity);

  if (humidity.relative_humidity > 0){
    doc.clear();
    doc["humidade"] = String(humidity.relative_humidity);
    enviarDados(doc, topicoHumidade);
  }
}

void enviarDados(DynamicJsonDocument jsonDoc, String topico) {
  connectMQTTServer();
  Serial.print("Sending payload: ");
  serializeJsonPretty(jsonDoc, Serial);

  //__ Envia o dado
  char buffer[256];
  size_t n = serializeJson(jsonDoc, buffer);
  if (client.publish(topico.c_str(), buffer, n)) {
    Serial.println("Publish ok");
  } else {
    Serial.println("Publish failed");
  }
}

void setup() {
  Serial.begin(115200);
  aht.begin();
  WiFiManager wifiManager;
  wifiManager.setAPCallback(configModeCallback);
  wifiManager.setSaveConfigCallback(saveConfigCallback);
  DEVICE_ID = String(WiFi.macAddress());
  clientId = DEVICE_ID;
  wifiManager.resetSettings();
  wifiManager.autoConnect("QuatroEstacoes"); // ATENÇÃO AUTOCONNECT DESLIGA OS SENSORES QUE USAM A PORTA ANALOGICA NÃO UTILIZAR

  connectMQTTServer();

  atualizaEnviaStart();
  pinMode(pino_a36, INPUT);
  pinMode(pin_a15, INPUT);
}

void loop() {
  client.loop();
  atualizaDados();
}