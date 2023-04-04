using UnityEngine;
using UnityEngine.SceneManagement;

public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
{
    public enum eMode { idel, move, attack, transition, knockback }

    [Header("Set in I")]
    public float speed = 5;
    public float attackDuration = 0.25f;
    public float atackDelay = 0.5f;
    public float transitionDelay = 0.5f;// Задержка перехода между
                                        // комнатам
    public int maxHealth = 10;
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;
    [Header("Set D")]
    public int dirHeld = -1;// Направление3 соответствующее
                            // удерживаемой клавише
    public int facing = 1;// Направление движения Дрея
    public eMode mode = eMode.idel;
    public int numKeys = 0;
    public bool invincible = false;
    public bool hasGrappler = false;
    public Vector3 lastSafeLoc;
    public int lastSafeFacing;
    public bool FaceRight = true;

    [SerializeField]
    private int _health;
    public int health
    {
        get { return _health; }
        set { _health = value; }
    }

    public float timeAtkDone = 0;
    private float timeAtkNext = 0;

    private float transitionDone = 0;
    private Vector2 transitionPos;
    private float knockbackDone = 0;
    private float invicibleDone = 0;
    private Vector3 knockbackVel;

    private SpriteRenderer sRend;

    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;

    private Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left,Vector3.down
    };
    private KeyCode[] keys = new KeyCode[]
    {
        KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow
    };
    private void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
        health = maxHealth;
        lastSafeLoc = transform.position;// Начальная позиция безопасна
        lastSafeFacing = facing;
    }
    void LateUpdate()
    {
        // Получить координаты узла сетки, с размером ячейки
        // в половину единицы, ближайшего к данному персонажу
        Vector2 rPos = GetRoomPosOnGrid(0.5f);// Размер ячейки в пол-единицы

        // Персонаж находится на плитке с дверью?
        int doorNum;
        for (doorNum = 0; doorNum < 4; doorNum++)
        {
            if (rPos == InRoom.DORS[doorNum])
            {
                break;
            }
        }

        if (doorNum > 3 || doorNum != facing) return;

        // Перейти в следующую комнату
        Vector2 rm = roomNum;
        switch (doorNum)
        {
            case 0:
                rm.x += 1;
                break;
            case 1:
                rm.y += 1;
                break;
            case 2:
                rm.x -= 1;
                break;
            case 3:
                rm.y -= 1;
                break;
        }

        // Проверить, можно ли выполнить переход в комнату rm
        if (rm.x >= 0 && rm.x <= InRoom.MAX_RM_X)
        {
            if (rm.y >= 0 && rm.y <= InRoom.MAX_RM_Y)
            {
                roomNum = rm;
                transitionPos = InRoom.DORS[(doorNum + 2) % 4];
                roomPos = transitionPos;
                lastSafeLoc = transform.position;
                lastSafeFacing = facing;
                mode = eMode.transition;
                transitionDone = Time.time + transitionDelay;
            }
        }
    }
    private void Update()
    {
        //поворот персонажа в зависемости куда он смотрит
        //____________________________________
        if (Input.GetAxis("Horizontal") > 0)
        {
            FaceRight = true;

            Quaternion rot = transform.rotation;
            rot.y = 0;
            transform.rotation = rot;
        }

        if (Input.GetAxis("Horizontal") < 0)
        {
            FaceRight = false;

            Quaternion rot = transform.rotation;
            rot.y = 180;
            transform.rotation = rot;
        }

        //____________________________________________

        // Проверить состояние неуязвимости и необходимость выполнить отбрасывание
        if (invincible && Time.time > invicibleDone)
        {
            invincible = false;
        }
        sRend.color = invincible ? Color.red : Color.white;
        if (mode == eMode.knockback)
        {
            rigid.velocity = knockbackVel;
            if (Time.time < knockbackDone)
            {
                return;
            }
        }

        if (mode == eMode.transition)
        {
            rigid.velocity = Vector3.zero;
            anim.speed = 0;
            roomPos = transitionPos;// Оставить Дрея на месте
            if (Time.time < transitionDone)
            {
                return;
            }
            // Следующая строка выполнится, только если Time.time >= transitionDone
            mode = eMode.idel;
        }

        //-—Обработка ввода с клавиатуры и управление режимами eMode
        dirHeld = -1;
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKey(keys[i])) dirHeld = i;
            // FaceRightPlayer();
        }

        // Нажата клавиша атаки
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= timeAtkNext)
        {
            mode = eMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + atackDelay;
        }

        // Завершить атаку, если время истекло
        if (Time.time >= timeAtkDone)
        {
            mode = eMode.idel;
        }

        // Выбрать правильный режим, если Дрей не атакует
        if (mode != eMode.attack)
        {
            if (dirHeld == -1)
            {
                mode = eMode.idel;
            }
            else
            {
                facing = dirHeld;
                mode = eMode.move;
            }
        }

        //-—Действия в текущем режиме-—
        Vector3 vel = Vector3.zero;
        switch (mode)
        {
            case eMode.idel:
                anim.Play("Idel1");
                // anim.speed = 0;
                break;
            case eMode.move:
                vel = directions[dirHeld];
                anim.Play("Run3");
                anim.speed = 1;
                break;
            case eMode.attack:
                anim.Play("Attack5");
                //anim.speed = 0;
                break;
        }
        rigid.velocity = vel * speed;
    }

    public int GetFacing()
    {
        return facing;
    }
    public bool moving
    {
        get
        {
            return mode == eMode.move;
        }
    }

    public float GetSpeed()
    {
        return speed;
    }
    public float gridMult
    {
        get { return inRm.gridMult; }
    }
    public Vector2 roomPos
    {
        get { return inRm.roomPos; }
        set { inRm.roomPos = value; }
    }
    public Vector2 roomNum
    {
        get { return inRm.roomNum; }
        set { inRm.roomNum = value; }
    }

    public int keyCount
    {
        get
        {
            return numKeys;
        }
        set { numKeys = value; }
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return inRm.GetRoomPosOnGrid(mult);
    }
    void OnCollisionEnter(Collision coll)
    {
        if (invincible)// Выйти, если Дрей пока неуязвим
        {
            return;
        }
        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
        if (dEf == null) return;// Если компонент DamageEffect отсутствует - выйти

        health -= dEf.damage;//Вычесть величину ущерба из уровня здоровья
        invincible = true;// Сделать Дрея неуязвимым
        invicibleDone = Time.time + invincibleDuration;
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (dEf.knockback)
        {
            // Выполнить отбрасывание 
            // Определить направление отбрасывания
            Vector3 delta = transform.position - coll.transform.position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                // Отбрасывание по горизонтали
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            }
            else
            {
                // Отбрасывание по вертикали
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }

            // Применить скорость отскока к компоненту Rigidbody
            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;

            // Установить режим knockback и время прекращения отбрасывания
            mode = eMode.knockback;
            knockbackDone = Time.time + knockbackDuration;
        }
    }
    void OnTriggerEnter(Collider coold)
    {
        PickUp pup = coold.GetComponent<PickUp>();
        if (pup == null) return;

        switch (pup.itemType)
        {
            case PickUp.eType.key:
                keyCount++;
                break;
            case PickUp.eType.health:
                health = Mathf.Min(health + 2, maxHealth);
                break;
            case PickUp.eType.grappler:
                hasGrappler = true;
                break;
        }
        Destroy(coold.gameObject);
    }
    public void ResetInRoom(int healthLoss = 0)
    {
        transform.position = lastSafeLoc;
        facing = lastSafeFacing;
        health -= healthLoss;

        invincible = true;// Сделать Дрея неуязвимым
        invicibleDone = Time.time + invincibleDuration;
    }
}
