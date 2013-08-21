// GSR sensor variables
 int sensorPin = 0; // select the input pin for the GSR
 int sensorValue; // variable to store the value coming from the sensor
 
// Time variables
 unsigned long time;
 int secForGSR;
 int curMillisForGSR;
 int preMillisForGSR;
 
void setup() {
  // Prepare serial port
  Serial.begin(9600);
  secForGSR = 1; // How often do we get a GSR reading
  curMillisForGSR = 0;
  preMillisForGSR = -1;
}

void loop() {
  time = millis();
 
  curMillisForGSR = time / (secForGSR * 1000);
  if (curMillisForGSR != preMillisForGSR) {
    // Read GSR sensor and send over Serial port
    sensorValue = analogRead(sensorPin);
    Serial.print(sensorValue);
    preMillisForGSR = curMillisForGSR;
  }
}
 

