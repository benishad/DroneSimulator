using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropStart : MonoBehaviour
{

    public GameObject[] propellers; // 프로펠러 오브젝트 배열
    public Vector3[] rotationAxes; // 각 프로펠러의 회전 축
    public float defaultRotationSpeed = 100f; // 기본 회전 속도
    private float currentRotationSpeed = 0f;

    private bool EngineData = false; // 엔진 데이터

    private bool isChecking = false;

    struct MyData
    {
        public int throttle;
        public int yaw;
        public int pitch;
        public int roll;
        public int AUX1;
        public int AUX2;
    }

    MyData data;


    void Start()
    {
        SaveData();
    }

    private void Update()
    {
        DataGet(); //데이터 불러오기

        // 시동 걸기
        if (data.throttle == 0 && data.yaw == 255 && !isChecking)
        {
            StartCoroutine(CheckEngineStart());
        }

        // 시동 끄기
        if (data.throttle == 0 && data.yaw == 0) 
        {
            StopAllCoroutines(); // Ensure the coroutine stops if conditions are no longer met
            isChecking = false;
            StartCoroutine(ChangeRotationSpeed(0f, 1f)); // 회전속도를 0으로 변경
        }

        // 시동이 켜진 상태에서만 명령 수행
        if (EngineData)
        {
            for (int i = 0; i < propellers.Length; i++)
            {
                propellers[i].transform.Rotate(rotationAxes[i], currentRotationSpeed * Time.deltaTime);
            }
        }
    }

    private IEnumerator CheckEngineStart()
    {
        isChecking = true;
        yield return new WaitForSeconds(2f);  // 2초 동안 대기

        // 2초 후에도 조건이 유효한지 확인
        if (data.throttle == 0 && data.yaw == 255)
        {
            EngineData = true;  // 시동 걸기
            StartCoroutine(ChangeRotationSpeed(defaultRotationSpeed, 1f)); // 회전속도를 임의로 설정한 회전속도로 변경
        }

        isChecking = false;
    }

    void serial_print()
    {
        Debug.Log("Throttle_2: " + data.throttle +
                  " Yaw_2: " + data.yaw +
                  " Pitch_2: " + data.pitch +
                  " Roll_2: " + data.roll +
                  " AUX1_2: " + data.AUX1 +
                  " AUX2_2: " + data.AUX2);
    }

    void DataGet()
    {
        // PlayerPrefs에서 저장된 throttle, yaw, pitch, roll, aux1, aux2 값을 불러옵니다.
        data.throttle = PlayerPrefs.GetInt("ThrottleValue");
        data.yaw = PlayerPrefs.GetInt("YawValue"); // 적절한 기본값 설정
        data.pitch = PlayerPrefs.GetInt("PitchValue"); // 적절한 기본값 설정
        data.roll = PlayerPrefs.GetInt("RollValue"); // 적절한 기본값 설정
        data.AUX1 = PlayerPrefs.GetInt("AUX1Value"); // 적절한 기본값 설정
        data.AUX2 = PlayerPrefs.GetInt("AUX2Value"); // 적절한 기본값 설정
    }

    // 데이터를 다른 스크립트에서 사용하기 위해 저장
    void SaveData()
    {
        PlayerPrefs.SetFloat("defaultRotationSpeedValue", defaultRotationSpeed);
        PlayerPrefs.SetInt("EnginData", EngineData ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 회전 속도를 부드럽게 변경하는 보간 함수
    private IEnumerator ChangeRotationSpeed(float targetSpeed, float duration)
    {
        float startSpeed = currentRotationSpeed;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            currentRotationSpeed = Mathf.Lerp(startSpeed, targetSpeed, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        currentRotationSpeed = targetSpeed;
    }
}