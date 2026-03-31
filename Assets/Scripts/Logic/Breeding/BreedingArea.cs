using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 사육장 클래스 입니다.
/// </summary>
public class BreedingArea : BaseMono , IFoodProvider
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private GameObject _cagePrefab;    //울타리로 사용될 프리팹
    [SerializeField] private GameObject _foodBoxPrefab; //먹이통으로 사용될 프리팹
    //[SerializeField] private AnimalObject _animalPrefab; // 동물 프리팹 (테스트용 실제로는 사용하지 않을 녀석)

    [Header("사이즈")]
    [SerializeField] private int _witdh;
    [SerializeField] private int _height;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Transform _foodBoxTr;        // 먹이통의 위치
    private List<AnimalObject> _animals; // 관리하는 동물들의 리스트
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // IFoodProvider 로 약속한 함수.
    // 동물에게 먹이통의 위치를 알려주는 함수이다.
    public Transform GetFoodBoxPosition()
    {
        return _foodBoxTr;
    }

    //사육장을 만드는 기능.
    public void MakeBreedingArea()
    {
        for (int i = -_witdh/2; i < _witdh/2; i++)
        {
            for (int j = -_height/2; j < _height/2; j++)
            {
                //울타리 세팅
                if (i == -_witdh / 2 || i == _witdh / 2 - 1 || j == -_height / 2 || j == _height / 2 - 1)
                {
                    GameObject tempGo = Instantiate(_cagePrefab);
                    tempGo.transform.SetParent(transform);
                    tempGo.transform.localPosition = new Vector3(i, j);
                }
            }
        }
        //먹이통 세팅
        GameObject tempGoFB = Instantiate(_foodBoxPrefab);
        _foodBoxTr = tempGoFB.transform;
        tempGoFB.transform.SetParent(transform);
        tempGoFB.transform.localPosition = GetRandomPos();
    }

    //동물의 ID를 입력하여 객체를 소환하는 기능
    public void SpawnAnimal(string id)
    {
        GameObject tempGo = FactoryManager.Ins.Animal.Spawn(id);
        if(!tempGo.GetComponent<AnimalObject>())
        {
            UDebug.Print("잘못된 객체가 생성되고 있습니다. 여기에는 AnimalObject가 반환되어야 합니다. 팩토리 확인");
            return;
        }

        _animals.Add(tempGo.GetComponent<AnimalObject>());
        tempGo.GetComponent<AnimalObject>().SetFoodProvider(this);
        
        tempGo.transform.SetParent(this.transform);
        tempGo.transform.localPosition = GetRandomPos();
    }

    //소환되는 동물의 위치를 사육장 내 랜덤한 위치로 조정하기 위함.
    private Vector3 GetRandomPos()
    {
        float posX = Random.Range(-_witdh / 2 + 1, _witdh / 2 - 1);
        float posY = Random.Range(-_height / 2 + 1, _height / 2 - 1);
        return new Vector3(posX, posY);
        #endregion

    }
    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        UDebug.IsNull(_cagePrefab);
        UDebug.IsNull(_foodBoxPrefab);

        _animals = new List<AnimalObject>();

        MakeBreedingArea();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
