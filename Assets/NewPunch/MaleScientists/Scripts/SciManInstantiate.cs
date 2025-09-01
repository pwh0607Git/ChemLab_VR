using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SciManInstantiate : MonoBehaviour
{
	private int faceT;
	private int skinT;
	private int eyeC;
	private int hairT;
	private int hairC;
	private bool glassesT;
	private int coatC;

	private bool tieT;
    private bool maskT;
    private bool stethoT;

    private int tieC;
	private int pantsC;
	private int shoesC;


	private SciManAssetsList assetsList;

	private SkinnedMeshRenderer skinnedMeshRenderer;

	public enum FaceType
	{
		FaceA,
		FaceB,
		FaceC,
		FaceD,
		FaceE

	}

	//public enum SkinType
	//{
	//	SkinA,
	//	SkinB,
	//	SkinC,
	//	SkinD,
	//	SkinE

	//}

	public enum EyeColor
	{
		Brown,
		Blue,
		Green,
		Black,
		Gray,
		LightBrown

	}

	public enum Hair
	{
		HairA,
		HairB,
		HairC,
		HairD,
		HairE,
		HairF,
		HairG,
		HairH,
		HairI
	}

	public enum HairColor
	{
		Blond,
		Brown,
		Gray,
		Brunete,
		Black
	}

	public enum Glasses
	{
		No,
		Glasses,
		SunGlasses

	}


	public enum Tie
	{
		Tie,
		Butterfly,
		No

	}

	


	public enum TieColor
	{
		Black,
		Red,
		Blue,
		RedBlue,
		Purple

	}

	

	public enum ShoesColor
	{
		Black,
		Brown,
		RedBrown,
		White
	}



	public Transform prefabObject;
    //
    public FaceType faceType;
    //public SkinType skinType;
    public EyeColor eyeCol;

    public Hair hair;
    public HairColor hairCol;
    public TieColor tieCol;
    public ShoesColor shoesCol;
    [Range(0, 11)]
    public int coatCol;

    [Range(0, 11)]
    public int pantsCol;

    public bool glasses;
    public bool tie;
    public bool mask;
    public bool stethoscope;
    // Use this for initialization
    void Start ()
	{
		Transform pref = Instantiate (prefabObject, gameObject.transform.position, gameObject.transform.rotation);
		hairC = (int)hairCol;
		eyeC = (int)eyeCol;
		glassesT = glasses;
		hairT = (int)hair;
		faceT = (int)faceType;
        skinT = (int)faceType;
        //skinT = (int)skinType;
		coatC = (int)coatCol;

		tieT = tie;

		tieC = (int)tieCol;
		pantsC = (int)pantsCol;
		shoesC = (int)shoesCol;
        maskT = (bool)mask;
        stethoT = (bool)stethoscope;


        pref.gameObject.GetComponent<SciManCustomize> ().charCustomize (faceT, skinT, eyeC, glassesT, hairT, hairC, tieT, tieC, pantsC, shoesC, coatC, maskT, stethoT);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
