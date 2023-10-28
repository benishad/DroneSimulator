using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropStart : MonoBehaviour
{
    public Rigidbody droneRigidbody; // 드론의 Rigidbody
    public float liftForce = 50f; // 드론이 상승하는 힘
    
    public GameObject[] propellers;
    public Vector3[] rotationAxes;
    public float defaultRotationSpeed = 100f;
    private float currentRotationSpeed = 0f;

    private bool EngineData = false;
    private bool isChecking = false;

    public float horizontalReturnSpeed = 1f; // 추가: 수평 상태로 돌아오는 속도
    public float torqueReturnSpeed = 1f; // 추가: 토크가 0으로 돌아오는 속도

    public float maxTiltAngle = 30f; //드론의 최대 기울기 각도

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
        DataGet();

        // 시동 걸기
        if (data.throttle == 0 && data.yaw == 255 && !isChecking)
        {
            StartCoroutine(CheckEngineStart());
        }

        // 시동 끄기
        if (data.throttle == 0 && data.yaw == 0)
        {
            StopAllCoroutines();
            isChecking = false;
            StartCoroutine(ChangeRotationSpeed(0f, 1f));
        }

        // 시동이 켜진 상태에서만 명령 수행
        if (EngineData)
        {

            // 드론 상승
            if (data.throttle > 10)
            {
               float appliedLiftForce = liftForce * ((data.throttle - 10) / 100f);
               droneRigidbody.AddForce(transform.up * appliedLiftForce);
            }
            //드론 좌우 회전
            if (data.yaw > 130) // 오른쪽으로 회전
            {
                droneRigidbody.AddTorque(transform.up * (data.yaw - 130) / 10f);
            } 
            else if (data.yaw < 125) // 왼쪽으로 회전
            {
                droneRigidbody.AddTorque(transform.up * -(125 - data.yaw) / 10f);
            }

            //드론 수평 유지
            if ((data.pitch >= 125 && data.pitch <= 130) && (data.roll >= 125 && data.roll <= 130))
            {
                Quaternion targetRotation = Quaternion.Euler(0, droneRigidbody.rotation.eulerAngles.y, 0); // 목표 회전(수평 상태, y축 회전 값은 현재 값 유지)
                droneRigidbody.rotation = Quaternion.Slerp(droneRigidbody.rotation, targetRotation, horizontalReturnSpeed * Time.deltaTime);

                // 토크를 서서히 0으로 만듦
                Vector3 currentTorque = droneRigidbody.angularVelocity;
                droneRigidbody.angularVelocity = Vector3.Lerp(currentTorque, Vector3.zero, torqueReturnSpeed * Time.deltaTime);
            }
            else
            {
                if (data.pitch > 130) // 드론후진
                {
                    droneRigidbody.AddTorque(transform.right * -(data.pitch - 130) / 10f);
                }
                else if (data.pitch < 125) // 드론전진
                {
                    droneRigidbody.AddTorque(transform.right * (125 - data.pitch) / 10f);
                }

                // 드론 좌우 이동
                if (data.roll > 130) // 왼쪽으로 이동
                {
                    droneRigidbody.AddTorque(transform.forward * (data.roll - 130) / 10f);
                } 
                else if (data.roll < 125) // 오른쪽으로 이동
                {
                    droneRigidbody.AddTorque(transform.forward * -(125 - data.roll) / 10f);
                }
            }

            for (int i = 0; i < propellers.Length; i++)
            {
                propellers[i].transform.Rotate(rotationAxes[i], currentRotationSpeed * Time.deltaTime);
            }

            LimitTiltAngle();   // 드론이 움직이는 최대 기울기 각도 제한 함수
        }
    }

    private void LimitTiltAngle() // 드론이 움직이는 최대 기울기 각도 제한 함수
    {
        Vector3 currentRotation = droneRigidbody.rotation.eulerAngles;

        if (currentRotation.x > 180f) currentRotation.x -= 360f;
        if (Mathf.Abs(currentRotation.x) > maxTiltAngle)
        {
            currentRotation.x = maxTiltAngle * Mathf.Sign(currentRotation.x);
        }

        if (currentRotation.z > 180f) currentRotation.z -= 360f;
        if (Mathf.Abs(currentRotation.z) > maxTiltAngle)
        {
            currentRotation.z = maxTiltAngle * Mathf.Sign(currentRotation.z);
        }

        droneRigidbody.rotation = Quaternion.Euler(currentRotation);
    }

    private IEnumerator CheckEngineStart()
    {
        isChecking = true;
        yield return new WaitForSeconds(2f);

        if (data.throttle == 0 && data.yaw == 255)
        {
            EngineData = true;
            StartCoroutine(ChangeRotationSpeed(defaultRotationSpeed, 1f));
        }

        isChecking = false;
    }

    void DataGet()
    {
        data.throttle = PlayerPrefs.GetInt("ThrottleValue");
        data.yaw = PlayerPrefs.GetInt("YawValue");
        data.pitch = PlayerPrefs.GetInt("PitchValue");
        data.roll = PlayerPrefs.GetInt("RollValue");
        data.AUX1 = PlayerPrefs.GetInt("AUX1Value");
        data.AUX2 = PlayerPrefs.GetInt("AUX2Value");
    }

    void SaveData()
    {
        PlayerPrefs.SetFloat("defaultRotationSpeedValue", defaultRotationSpeed);
        PlayerPrefs.SetInt("EnginData", EngineData ? 1 : 0);
        PlayerPrefs.Save();
    }

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