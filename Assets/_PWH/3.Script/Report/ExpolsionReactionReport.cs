using UnityEngine;

public class ExpolsionReactionReport : EPReport
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Pen"))
        {
            WriteResult();
        }   
        
        if (collision.gameObject.tag.Equals("NPC"))
        {
            //리포트 제출 텍스트 출력
            GeminiAPIManager.Instance.SendMessage("Report_Explosion");
        }
    }
}