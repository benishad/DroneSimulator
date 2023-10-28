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

            for (int i = 0; i < propellers.Length; i++)
            {
                propellers[i].transform.Rotate(rotationAxes[i], currentRotationSpeed * Time.deltaTime);
            }
        }
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