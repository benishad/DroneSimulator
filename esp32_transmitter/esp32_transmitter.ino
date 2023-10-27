//조종기 ESP32
//드론 시뮬레이터를 위한 드론 조종장치 중 조종기 코드
#include <SPI.h>
#include <RF24.h>

const uint64_t pipeOut = 0xABCD1234567890EFLL;   //주소값 설정

RF24 radio(4, 5); // 4번 ce 5번 csn

struct MyData {   //전송할 데이터 구조체 생성
  byte throttle;
  byte yaw;
  byte pitch;
  byte roll;
  byte AUX1;
  byte AUX2;
};

MyData data;

void resetData() 
{
  data.throttle = 0;
  data.yaw = 127;
  data.pitch = 127;
  data.roll = 127;
  data.AUX1 = 0;
  data.AUX2 = 0;
}

void setup()
{
  Serial.begin(9600);
  radio.begin();
  //radio.setAutoAck(false);
  //radio.setDataRate(RF24_250KBPS);
  radio.openWritingPipe(pipeOut);
  radio.stopListening(); // Listening을 멈춤
  resetData();

  pinMode(33, INPUT);
  pinMode(32, INPUT);
}

/**************************************************/
//조이스틱에 중간 값이 조이스틱마다 저항 값에 차이가 있어서 그 값을 맞춰주는 함수
int mapJoystickValues(int val, int lower, int middle, int upper, bool reverse)
{
  val = constrain(val, lower, upper);
  if (val < middle)
    val = map(val, lower, middle, 0, 128);
  else
    val = map(val, middle, upper, 128, 255);
  return (reverse ? 255 - val : val);
}

void loop()
{
  data.throttle = mapJoystickValues(analogRead(34), 80, 1850, 4050, true);//34
  data.yaw      = mapJoystickValues(analogRead(14), 0, 2045, 4095, true);//35
  data.pitch    = mapJoystickValues(analogRead(15), 0, 1765, 4095, true);//32
  data.roll     = mapJoystickValues(analogRead(35), 0, 2045, 4095, false);//33
  data.AUX1     = digitalRead(32);//26
  data.AUX2     = digitalRead(33);//25

  radio.write(&data, sizeof(MyData));

//   송신하는 값을 시리얼 모니터에 출력
  Serial.print("조종기 값 -->  ");
  Serial.print("Throttle: ");
  Serial.print(data.throttle);
  Serial.print("  Yaw: ");
  Serial.print(data.yaw);
  Serial.print("  Pitch: ");
  Serial.print(data.pitch);
  Serial.print("  Roll: ");
  Serial.print(data.roll);
  Serial.print("  AUX1: ");
  Serial.print(data.AUX1);
  Serial.print("  AUX2: ");
  Serial.println(data.AUX2);

  //delay(100); // 일정 시간 간격으로 송신 (500ms = 0.5초)
}
