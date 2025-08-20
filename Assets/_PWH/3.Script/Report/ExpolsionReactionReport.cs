using UnityEngine;

public class ExpolsionReactionReport : EPReport
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Pen"))
        {
            WriteResult();
        }   
    }
}