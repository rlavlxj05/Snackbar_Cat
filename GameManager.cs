using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public GameObject customerCat; // �մԳ� ������
    public GameObject kitchenCat; // �˹ٳ� ������

    public Transform[] seats; // �ڸ�
    public GameObject[] customers; // �մ�
    public int emptySeats; // ���ڸ�

    public GameObject[] food; // ����
    public Queue<GameObject> kitchenQueue = new Queue<GameObject>(); // �մԳ� ��⿭

    public Queue<GameObject> kitchen0 = new Queue<GameObject>();//������1
    public Queue<GameObject> kitchen1 = new Queue<GameObject>();//������2

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
            GameObject newObject = Instantiate(customerCat, new Vector3(-4, 4, 0), Quaternion.identity); // ��ü ������ġ
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
        customerMove.gameManager = this; // �մԳɿ��� GameManager ����
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
        kitchenQueue.Enqueue(customer); // �մԳ��� �˹ٳ� ��⿭�� �߰�
    }
}

//�մԳ�
public class Customer : MonoBehaviour
{
    public GameObject food; // ���ϴ� ����
    public int foodNum; //���ϴ� ���� ����

    public Transform target; // �ڸ�
    public float speed = 1f; // �̵��ӵ�

    public bool Exit;
    private bool delivered = false;
    private bool arrived = false;

    public GameManager gameManager; // GameManager ����

    private void Update()
    {
        if (target != null && !arrived)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (transform.position == target.position)
            {
                Debug.Log("�մ��� �������� �����߽��ϴ�.");
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

//�ֹ��
public class KitchenCat : MonoBehaviour
{
    GameManager gameManager;

    public Queue<GameObject> jobQueue = new Queue<GameObject>(); //���� ��⿭

    public float speed = 1f; //�ֹ�� �̵��ӵ�
    public float time; //�ֹ���� �ð�

    public bool IsMoveing = false; //�̵�

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

    //�մ� ��⿭���� �� �տ� �ִ� ��ü ��������
    IEnumerator jobQueueA()
    {
        GameObject firstCustomer = gameManager.kitchenQueue.Dequeue();
        jobQueue.Enqueue(firstCustomer);

        yield return null;

        StartCoroutine(jobQueueB());
    }

    //�մԿ��� �̵��ϴ� �ý���
    IEnumerator jobQueueB()
    {
        IsMoveing = true;
        Vector3 targetPosition = jobQueue.Peek().transform.position - Vector3.up;

        while (IsMoveing)
        {
            // �ֹ���� �Ʒ��� �̵��մϴ�.
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            yield return null;

            if (transform.position == targetPosition)
            {
                Debug.Log("�ֹ���� �մ��� ��ġ�� �̵� �Ϸ�!");
                Customer customerScript = jobQueue.Peek().GetComponent<Customer>();
                jobQueue.Enqueue(customerScript.food);

                IsMoveing = false;
            }
        }
        StartCoroutine(jobQueueC());
    }

    //�����ǿ� ��� �ִ��� Ȯ���ϴ� �ý���
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
                            Debug.Log("�������� �����ϴ�.");
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
                            Debug.Log("�������� �����ϴ�.");
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


    //�����Ƿ� �̵��ϴ� �ý���
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

    //���� ��� �ý���
    IEnumerator jobQueueE()
    {
        IsMoveing = true;
        Vector3 targetPosition = jobQueue.Peek().transform.position - Vector3.up;

        while (IsMoveing)
        {
            // �ֹ���� �Ʒ��� �̵��մϴ�.
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            yield return null;

            if (transform.position == targetPosition)
            {
                Debug.Log("�ֹ���� �մ��� ��ġ�� �̵� �Ϸ�!");
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
                    Debug.Log("1���̳�?");
                }
                else if(i == 1)
                {
                    gameManager.kitchen1.Enqueue(job);
                    Debug.Log("2���̳�?");

                }
            }
        }
    }
}