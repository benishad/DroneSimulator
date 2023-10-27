using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class arduino : MonoBehaviour
{
    SerialPort serial = new SerialPort("COM13", 115200); // 시리얼 포트 설정
    //public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    //전송할 데이터 구조체 생성
    struct MyData {   
    byte throttle;
    byte yaw;
    byte pitch;
    byte roll;
    byte AUX1;
    byte AUX2;
    };

    MyData data;
    void Start()
    {
        serial.Open(); // 시리얼 포트 열기
    }

    void Update()
    {
        if (serial.IsOpen) // 시리얼 포트가 열려있다면
        {
            string data = serial.ReadLine(); // 아두이노에서 전송한 값을 읽어옵니다.
            String inString = Serial2.readStringUntil('\n'); //개행을 기준으로 데이터를 읽음

            //데이터의 오류 값을 걸러내기 위한 필터링
            if(inString.length() == 26)
            {
                data.throttle = inString.substring(1, 4).toInt();    //받은 스트링 값을 분할 후 인티져로 변환하여 저장
                data.yaw = inString.substring(5, 8).toInt();
                data.pitch = inString.substring(9, 12).toInt();
                data.roll = inString.substring(13, 16).toInt();
                data.AUX1 = inString.substring(17, 20).toInt();
                data.AUX2 = inString.substring(21, 24).toInt();
                serial_print();
            }
            else
                Serial2.println("error");

            // string[] values = data.Split(','); // 쉼표(,)를 구분자로 사용하여 값을 분리합니다.

            // // X, Y, Z 값에서 X, Y, Z 값을 이용하여 오브젝트를 회전합니다.
            // float x = float.Parse(values[0]);
            // float y = float.Parse(values[1]);
            // float z = float.Parse(values[2]);

            // transform.rotation = Quaternion.Euler(x * rotationSpeed, y * rotationSpeed, z * rotationSpeed);
        }
    }

    void OnApplicationQuit()
    {
        serial.Close(); // 어플리케이션이 종료될 때 시리얼 포트를 닫아줍니다.
    }

    void serial_print()
    { 
        Serial2.print("Throttle: ");
        Serial2.print(data.throttle);
        Serial2.print("  Yaw: ");
        Serial2.print(data.yaw);
        Serial2.print("  Pitch: ");
        Serial2.print(data.pitch);
        Serial2.print("  Roll: ");
        Serial2.print(data.roll);
        Serial2.print("  AUX1: ");
        Serial2.print(data.AUX1);
        Serial2.print("  AUX2: ");
        Serial2.println(data.AUX2); 
    }
}