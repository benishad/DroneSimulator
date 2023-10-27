//esp32 드론 시뮬레이터 수신기
//라디오 통신으로 받은 값을 처리하여 컴퓨터로 시리얼 전송
#include <SPI.h>
#include <RF24.h>

const uint64_t pipeIn = 0xABCD1234567890EFLL;   //주소값 설정

char buf[18]; //  

RF24 radio(4, 5); // GPIO18 for CE, GPIO5 for CSN

struct MyData {
  byte throttle;
  byte yaw;
  byte pitch;
  byte roll;
  byte AUX1;
  byte AUX2;
};

MyData receivedData;

void setup() 
{
  Serial.begin(9600);
  radio.begin();
  radio.openReadingPipe(1, pipeIn); // Set the same address used in the transmitter
  radio.startListening();
}

void loop() 
{ 
  if(radio.available()) 
  {
    radio.read(&receivedData, sizeof(MyData));
//    test_data();
    serial_trans();
    delay(10);
  }
}

void test_data()
{
  Serial.print("Received - Throttle: ");
  Serial.print(receivedData.throttle);
  Serial.print("  Yaw: ");
  Serial.print(receivedData.yaw);
  Serial.print("  Pitch: ");
  Serial.print(receivedData.pitch);
  Serial.print("  Roll: ");
  Serial.print(receivedData.roll);
  Serial.print("  AUX1: ");
  Serial.print(receivedData.AUX1);
  Serial.print("  AUX2: ");
  Serial.println(receivedData.AUX2);
}

void serial_trans()
{
  Serial.print("A");
  sprintf(buf, "%03d", receivedData.throttle);
  Serial.print(buf);
  Serial.print("B");
  sprintf(buf, "%03d", receivedData.yaw);
  Serial.print(buf);
  Serial.print("C");
  sprintf(buf, "%03d", receivedData.pitch);
  Serial.print(buf);
  Serial.print("D");
  sprintf(buf, "%03d", receivedData.roll);
  Serial.print(buf);
  Serial.print("E");
  sprintf(buf, "%03d", receivedData.AUX1);
  Serial.print(buf);
  Serial.print("F");
  sprintf(buf, "%03d", receivedData.AUX2);
  Serial.print(buf);
  Serial.println("G");
}
