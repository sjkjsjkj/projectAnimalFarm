using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class BreedingArea : BaseMono , IFoodProvider
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private GameObject _cagePrefab;
    [SerializeField] private GameObject _foodBoxPrefab;

    [SerializeField] private AnimalObject _animalPrefab;

    [Header("사이즈")]
    [SerializeField] private int _witdh;
    [SerializeField] private int _height;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Transform _foodBoxTr;
    private List<AnimalObject> _animals;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public Transform GetFoodBoxPosition()
    {
        return _foodBoxTr;
    }
    public void MakeBreedingArea()
    {
        for (int i = 0; i < _witdh; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                if(i==0 || i==_witdh-1 || j==0 || j==_height-1)
                {
                    GameObject tempGo = Instantiate(_cagePrefab);
                    tempGo.transform.SetParent(transform);
                    tempGo.transform.localPosition = new Vector3(i, j);
                }
            }
        }
        GameObject tempGoFB = Instantiate(_foodBoxPrefab);
        _foodBoxTr = tempGoFB.transform;
        tempGoFB.transform.SetParent(transform);
        tempGoFB.transform.localPosition = new Vector3(Random.Range(2,_witdh-2),Random.Range(2, _height-2));
    }
    public void SpawnAnimal()
    {

    }
    public void SpawnAnimal(string id)
    {
        AnimalSO tempDataUnitSO = Database.Ins.Animal.FindData(id);

        AnimalObject tempGo = Instantiate(_animalPrefab);
        if (!(tempDataUnitSO as AnimalSO))
        {
            UDebug.Print($"읽어온 데이터에 AnimalSO가 없음.", LogType.Warning);
        }
        tempGo.GetComponent<AnimalObject>().SetInfo(tempDataUnitSO as AnimalSO);
        tempGo.GetComponent<AnimalObject>().SetFoodProvider(this);

        _animals.Add(tempGo);

        tempGo.transform.SetParent(this.transform);
        tempGo.transform.localPosition = Vector3.zero;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        UDebug.IsNull(_cagePrefab);

        _animals = new List<AnimalObject>();

        MakeBreedingArea();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
