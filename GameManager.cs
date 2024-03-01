using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public GameObject customerCat; // 손님냥 프리팹
    public GameObject kitchenCat; // 알바냥 프리팹

    public Transform[] seats; // 자리
    public GameObject[] customers; // 손님
    public int emptySeats; // 빈자리

    public GameObject[] food; // 음식
    public Queue<GameObject> kitchenQueue = new Queue<GameObject>(); // 손님냥 대기열

    public Queue<GameObject> kitchen0 = new Queue<GameObject>();//조리실1
    public Queue<GameObject> kitchen1 = new Queue<GameObject>();//조리실2

    public void StartButton()
    {
        InvokeRepeating("CreateObject", 0f, 1f);
        NewKitchenCat();

    }
    public void kitchenObj()
    {
        GameObject kitchenObj0_1 = Instantiate(food[0], new Vector3(-1, -3, 0), Quaternion.identity);
        kitchen0.Enqueue(kitchenObj0_1);
        GameObject kitchenObj0_2 = Instantiate(food[0], new Vector3(0, -3, 0), Quaternion.identity);
        kitchen0.Enqueue(kitchenObj0_2);
        GameObject kitchenObj1 = Instantiate(food[1], new Vector3(1, -3, 0), Quaternion.identity);
        kitchen1.Enqueue(kitchenObj1);
    }
    private void NewKitchenCat()
    {
        GameObject newKitchenCat = Instantiate(kitchenCat, new Vector3(0, -1, 0), Quaternion.identity);
        newKitchenCat.AddComponent<KitchenCat>();
    }
    public void btttt()
    {
        foreach (GameObject item in kitchenQueue)
        {
            Debug.Log(item.name);
        }

        kitchenObj();
    }
    private void CreateObject()
    {
        if (emptySeats > 0)
        {
            GameObject newObject = Instantiate(customerCat, new Vector3(-4, 4, 0), Quaternion.identity); // 객체 생성위치
            AddCustomerMove(newObject, FindEmptySeat());
            AddCustomerToArray(newObject);

            int randomFood = Random.Range(0, food.Length);
            Customer customer = newObject.GetComponent<Customer>();
            customer.food = food[randomFood];
            customer.foodNum = Random.Range(1, 3);

            emptySeats--;
        }
    }

    private void AddCustomerMove(GameObject obj, Transform targetSeat)
    {
        var customerMove = obj.AddComponent<Customer>();
        customerMove.target = targetSeat;
        customerMove.gameManager = this; // 손님냥에게 GameManager 연결
    }

    private Transform FindEmptySeat()
    {
        for (int i = 0; i < seats.Length; i++)
        {
            if (customers[i] == null)
            {
                return seats[i];
            }
        }
        return null;
    }

    private void AddCustomerToArray(GameObject newCustomer)
    {
        for (int i = 0; i < customers.Length; i++)
        {
            if (customers[i] == null)
            {
                customers[i] = newCustomer;
                return;
            }
        }
    }
    public void AddToKitchenQueue(GameObject customer)
    {
        kitchenQueue.Enqueue(customer); // 손님냥을 알바냥 대기열에 추가
    }
}

//손님냥
public class Customer : MonoBehaviour
{
    public GameObject food; // 원하는 음식
    public int foodNum; //원하는 음식 개수

    public Transform target; // 자리
    public float speed = 1f; // 이동속도

    public bool Exit;
    private bool delivered = false;
    private bool arrived = false;

    public GameManager gameManager; // GameManager 참조

    private void Update()
    {
        if (target != null && !arrived)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (transform.position == target.position)
            {
                Debug.Log("손님이 목적지에 도착했습니다.");
                gameManager.AddToKitchenQueue(gameObject);
                arrived = true;
            }
        }

        if (Exit && !delivered)
        {
            Vector3 exitPos = new Vector3(4f, 4f, 0f);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(4f, 4f, 0f), speed * Time.deltaTime);

            if (transform.position == exitPos)
            {
                gameManager.emptySeats++; 
                Destroy(gameObject);
                delivered = true;
            }
        }
    }

}

//주방냥
public class KitchenCat : MonoBehaviour
{
    GameManager gameManager;

    public Queue<GameObject> jobQueue = new Queue<GameObject>(); //할일 대기열

    public float speed = 1f; //주방냥 이동속도
    public float time; //주방냥의 시간

    public bool IsMoveing = false; //이동

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        StartCoroutine(JobSystem());
    }

    IEnumerator JobSystem()
    {
        while (gameManager.kitchenQueue.Count == 0)
        {
            yield return null;
        }
        StartCoroutine(jobQueueA());
    }

    //손님 대기열에서 맨 앞에 있는 객체 가져오기
    IEnumerator jobQueueA()
    {
        GameObject firstCustomer = gameManager.kitchenQueue.Dequeue();
        jobQueue.Enqueue(firstCustomer);

        yield return null;

        StartCoroutine(jobQueueB());
    }

    //손님에게 이동하는 시스템
    IEnumerator jobQueueB()
    {
        IsMoveing = true;
        Vector3 targetPosition = jobQueue.Peek().transform.position - Vector3.up;

        while (IsMoveing)
        {
            // 주방냥이 아래로 이동합니다.
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            yield return null;

            if (transform.position == targetPosition)
            {
                Debug.Log("주방냥이 손님의 위치로 이동 완료!");
                Customer customerScript = jobQueue.Peek().GetComponent<Customer>();
                jobQueue.Enqueue(customerScript.food);

                IsMoveing = false;
            }
        }
        StartCoroutine(jobQueueC());
    }

    //조리실에 비어 있는지 확인하는 시스템
    IEnumerator jobQueueC()
    {
        GameObject foodObj = jobQueue.ToArray()[1];
        IsMoveing = true;
        for (int i = 0; i < gameManager.food.Length; i++)
        {
            if (gameManager.food[i] == foodObj)
            {
                if (i == 0)
                {
                    while (IsMoveing)
                    {
                        if (gameManager.kitchen0.Count == 0)
                        {
                            Debug.Log("조리실이 없습니다.");
                        }
                        else
                        {
                            GameObject addKitchen0 = gameManager.kitchen0.Dequeue();
                            jobQueue.Enqueue(addKitchen0);
                            IsMoveing = false;
                        }

                        yield return null;
                    }
                }
                else if (i == 1)
                {
                    while (IsMoveing)
                    {
                        if (gameManager.kitchen1.Count == 0)
                        {
                            Debug.Log("조리실이 없습니다.");
                        }
                        else
                        {
                            GameObject addKitchen0 = gameManager.kitchen1.Dequeue();
                            jobQueue.Enqueue(addKitchen0);

                            IsMoveing = false;
                        }

                        yield return null;
                    }
                }
            }
        }
        StartCoroutine(jobQueueD());
    }


    //조리실로 이동하는 시스템
    IEnumerator jobQueueD()
    {
        IsMoveing = true;
        Vector3 targetPosition = jobQueue.ToArray()[2].transform.position + Vector3.up;

        while (IsMoveing)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            yield return null;
            if (transform.position == targetPosition)
            {
                asd();
                IsMoveing = false;
            }
        }
        StartCoroutine(jobQueueE());
    }

    //음식 배달 시스템
    IEnumerator jobQueueE()
    {
        IsMoveing = true;
        Vector3 targetPosition = jobQueue.Peek().transform.position - Vector3.up;

        while (IsMoveing)
        {
            // 주방냥이 아래로 이동합니다.
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            yield return null;

            if (transform.position == targetPosition)
            {
                Debug.Log("주방냥이 손님의 위치로 이동 완료!");
                jobQueue.Peek().GetComponent<Customer>().Exit = true;
                IsMoveing = false;
            }
        }
        jobQueue.Clear();
        StartCoroutine(JobSystem());
    }
    void asd()
    {
        GameObject foodObj = jobQueue.ToArray()[1];
        GameObject job = jobQueue.ToArray()[2];

        IsMoveing = true;
        for (int i = 0; i < gameManager.food.Length; i++)
        {
            if (gameManager.food[i] == foodObj)
            {
                if (i == 0)
                {
                   gameManager.kitchen0.Enqueue(job);
                    Debug.Log("1번이네?");
                }
                else if(i == 1)
                {
                    gameManager.kitchen1.Enqueue(job);
                    Debug.Log("2번이네?");

                }
            }
        }
    }
}