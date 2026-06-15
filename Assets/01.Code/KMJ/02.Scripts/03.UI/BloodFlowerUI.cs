
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BloodFlowerUI : MonoBehaviour
{
    [Header("Lerp 속도 조절")]
    [SerializeField] private float increasingValue = 5f;

    [Header("UI 꽃잎 리스트 (자동 할당됨)")]
    public List<Image> bloodFlowerUIs = new List<Image>();

    [SerializeField] private List<float> flowerCountList;

    [SerializeField] private List<GameObject> bloodFlowerEndList;
    
    private float _flowerValue = 1;         
    private float _modifierFlowerValue = 1;

    private float testFloat = 0;
    
    private void Awake()
    {
        bloodFlowerUIs = GetComponentsInChildren<Image>().ToList();
        for (int i = 0; i < bloodFlowerEndList.Count; i++)
            bloodFlowerEndList[i].SetActive(false);
    }

    private void Update()
    {
        LerpUIValue();
        SetFlower(); 
    }

    private void LerpUIValue()
    {
        _flowerValue = Mathf.Lerp(_flowerValue, _modifierFlowerValue, Time.deltaTime * increasingValue);
    }

    public void SetUIValue(float value)
    {
        _modifierFlowerValue = Mathf.Clamp(value, 0, 1000); 
    }

    private void SetFlower()
    {
        if (bloodFlowerUIs == null || bloodFlowerUIs.Count < 4)
            return;


        for (int i = 0; i < bloodFlowerUIs.Count; i++)
            bloodFlowerUIs[i].fillAmount = 0;
        
        for (int i = 0; i < bloodFlowerEndList.Count; i++)
            bloodFlowerEndList[i].SetActive(false);

        switch (_flowerValue)
        {
            case <= 100f: 
                Count100();
                break;

            case <= 200f: 
                Count200();
                break;

            case <= 300f: 
                Count300();
                break;
            case <= 400f: 
                Count400();
                break;
            case <= 500f: 
                Count500();
                break;
            case <= 600f: 
                Count600();
                break;
            case <= 700f: 
                Count700();
                break;
            case <= 800f: 
                Count800();
                break;
            case <= 900f: 
                Count900();
                break;
            default:
                Count1000();
                break;
        }
    }

    private void Count100()
    {
        bloodFlowerUIs[0].fillAmount = Mathf.InverseLerp(0, 100f, _flowerValue) - 0.2f;
    }

    private void Count200()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerUIs[0].fillAmount = 0.8f;
        bloodFlowerUIs[1].fillAmount = Mathf.InverseLerp(100f, 200f, _flowerValue) - 0.2f;
    }

    private void Count300()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = Mathf.InverseLerp(200f, 300f, _flowerValue) - 0.2f;
    }

    private void Count400()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerEndList[2].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = 0.8f;
        bloodFlowerUIs[3].fillAmount = Mathf.InverseLerp(300f, 400f, _flowerValue) - 0.2f;
    }

    private void Count500()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerEndList[2].SetActive(true);
        bloodFlowerEndList[3].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = 0.8f;
        bloodFlowerUIs[3].fillAmount = 0.8f;
        bloodFlowerUIs[4].fillAmount = Mathf.InverseLerp(400f, 500f, _flowerValue) - 0.2f;
    }

    private void Count600()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerEndList[2].SetActive(true);
        bloodFlowerEndList[3].SetActive(true);
        bloodFlowerEndList[4].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = 0.8f;
        bloodFlowerUIs[3].fillAmount = 0.8f;
        bloodFlowerUIs[4].fillAmount = 0.8f;
        bloodFlowerUIs[5].fillAmount = Mathf.InverseLerp(500f, 600f, _flowerValue) - 0.2f;
    }

    private void Count700()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerEndList[2].SetActive(true);
        bloodFlowerEndList[3].SetActive(true);
        bloodFlowerEndList[4].SetActive(true);
        bloodFlowerEndList[5].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = 0.8f;
        bloodFlowerUIs[3].fillAmount = 0.8f;
        bloodFlowerUIs[4].fillAmount = 0.8f;
        bloodFlowerUIs[5].fillAmount = 0.8f;
        bloodFlowerUIs[6].fillAmount = Mathf.InverseLerp(600f, 700f, _flowerValue) - 0.2f;
    }

    private void Count800()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerEndList[2].SetActive(true);
        bloodFlowerEndList[3].SetActive(true);
        bloodFlowerEndList[4].SetActive(true);
        bloodFlowerEndList[5].SetActive(true);
        bloodFlowerEndList[6].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = 0.8f;
        bloodFlowerUIs[3].fillAmount = 0.8f;
        bloodFlowerUIs[4].fillAmount = 0.8f;
        bloodFlowerUIs[5].fillAmount = 0.8f;
        bloodFlowerUIs[6].fillAmount = 0.8f;
        bloodFlowerUIs[7].fillAmount = Mathf.InverseLerp(700f, 800f, _flowerValue) - 0.2f;
    }

    private void Count900()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerEndList[2].SetActive(true);
        bloodFlowerEndList[3].SetActive(true);
        bloodFlowerEndList[4].SetActive(true);
        bloodFlowerEndList[5].SetActive(true);
        bloodFlowerEndList[6].SetActive(true);
        bloodFlowerEndList[7].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = 0.8f;
        bloodFlowerUIs[3].fillAmount = 0.8f;
        bloodFlowerUIs[4].fillAmount = 0.8f;
        bloodFlowerUIs[5].fillAmount = 0.8f;
        bloodFlowerUIs[6].fillAmount = 0.8f;
        bloodFlowerUIs[7].fillAmount = 0.8f;
        bloodFlowerUIs[8].fillAmount = Mathf.InverseLerp(800f, 900f, _flowerValue) - 0.2f;
    }

    private void Count1000()
    {
        bloodFlowerEndList[0].SetActive(true);
        bloodFlowerEndList[1].SetActive(true);
        bloodFlowerEndList[2].SetActive(true);
        bloodFlowerEndList[3].SetActive(true);
        bloodFlowerEndList[4].SetActive(true);
        bloodFlowerEndList[5].SetActive(true);
        bloodFlowerEndList[6].SetActive(true);
        bloodFlowerEndList[7].SetActive(true);
        bloodFlowerEndList[8].SetActive(true);
        bloodFlowerUIs[0].fillAmount =  0.8f;
        bloodFlowerUIs[1].fillAmount =  0.8f;
        bloodFlowerUIs[2].fillAmount = 0.8f;
        bloodFlowerUIs[3].fillAmount = 0.8f;
        bloodFlowerUIs[4].fillAmount = 0.8f;
        bloodFlowerUIs[5].fillAmount = 0.8f;
        bloodFlowerUIs[6].fillAmount = 0.8f;
        bloodFlowerUIs[7].fillAmount = 0.8f;
        bloodFlowerUIs[8].fillAmount = 0.8f;
        bloodFlowerUIs[9].fillAmount = Mathf.InverseLerp(900f,1000f, _flowerValue) - 0.2f;

        if (_modifierFlowerValue >= 1000)
        {
            bloodFlowerEndList[9].SetActive(true);
        }
    }
}
