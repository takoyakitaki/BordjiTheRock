using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed = 5f;
    public bool isHit = false; // <-- เพิ่มตัวแปรนี้เข้ามาเพื่อเก็บสถานะ

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    public void HitNote()
    {
        isHit = true; // <-- สับสวิตช์บอกว่าโน้ตนี้ถูกผู้เล่นตีไปแล้ว
        Destroy(gameObject);
    }

    public void MissNote()
    {
        Destroy(gameObject);
    }
}