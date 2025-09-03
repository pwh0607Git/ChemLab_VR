using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SciManCustomize : MonoBehaviour
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

    void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void charCustomize (int faceTy, int skinTy, int eyeCo, bool glassesTy, int hairTy, int hairCo, bool tieTy, int tieCo, int pantsCo, int shoesCo, int coatCo, bool mask, bool stetho)
	{
		Material[] mat;
		assetsList = gameObject.GetComponent<SciManAssetsList> ();

		foreach (Transform child in assetsList.CoatObj.transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = assetsList.CoatMaterials [coatCo];
		}

		foreach (Transform child in assetsList.EyeObj.transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = assetsList.Eye_Materials [eyeCo];
		}
		foreach (Transform child in assetsList.TieObj.transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = assetsList.TieMaterials [tieCo];
		}

		//Pants
		foreach (Transform child in assetsList.PantsObj.transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = assetsList.PantsMaterials [pantsCo];
		}
		//SHoes
		foreach (Transform child in assetsList.ShoesObj.transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = assetsList.ShoesMaterials [shoesCo];
		}

		//HairA==========================
		foreach (Transform child in assetsList.HairObjects [0].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairA_Materials [hairCo];

		}

		//HairB==========================
		foreach (Transform child in assetsList.HairObjects [1].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairB_Materials [hairCo];

		}

		//HairC==========================
		foreach (Transform child in assetsList.HairObjects [2].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairC_Materials [hairCo];

		}

		//HairD==========================
		foreach (Transform child in assetsList.HairObjects [3].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairD_Materials [hairCo];

		}

		//HairE==========================
		foreach (Transform child in assetsList.HairObjects [4].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairE_Materials [hairCo];

		}

		//HairF==========================
		foreach (Transform child in assetsList.HairObjects [5].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairF_Materials [hairCo];

		}

		//HairG==========================
		foreach (Transform child in assetsList.HairObjects [6].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairG_Materials [hairCo];

		}

		//HairH==========================
		foreach (Transform child in assetsList.HairObjects [7].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairH_Materials [hairCo];

		}

		//HairI==========================
		foreach (Transform child in assetsList.HairObjects [8].transform) {

			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();

			skinRend.material = assetsList.HairI_Materials [hairCo];

		}




		assetsList.TieObj.SetActive (false);

		//Set hair
		for (int i = 0; i < assetsList.HairObjects.Length; i++) {
			assetsList.HairObjects [i].SetActive (false);
		}
		assetsList.HairObjects [hairTy].SetActive (true);

		//Set glasses
		for (int i = 0; i < assetsList.GlassesObjects.Length; i++) {
			assetsList.GlassesObjects [i].SetActive (false);
		}
		if (glassesTy) {
			assetsList.GlassesObjects [faceTy].SetActive (true);
//			
		}

        //Set mask
        if (assetsList.MaskObjects[0] != null)
		{
            for (int i = 0; i < assetsList.MaskObjects.Length; i++)
        {
            assetsList.MaskObjects[i].SetActive(false);
        }
        if (mask)
        {
            assetsList.MaskObjects[faceTy].SetActive(true);
            //			
        }
		}
            

		// Set Tie

        if (tieTy) {
			assetsList.TieObj.SetActive (true);
			//			
		}

		// Set Stethoscope
		if (assetsList.StethoscopeObj != null)
		{
			if (stetho)
			{
				assetsList.StethoscopeObj.SetActive(true);
				//			
			}
			else
			{
				assetsList.StethoscopeObj.SetActive(false);
			}
		}
        // set face
        for (int i = 0; i < assetsList.FaceObjects.Length; i++) {
			assetsList.FaceObjects [i].SetActive (false);
		}
		assetsList.FaceObjects [faceTy].SetActive (true);
		foreach (Transform child in assetsList.FaceObjects [faceTy].transform) {

			string oName = child.gameObject.name;
			string hName = oName.Substring (0, 1);

			if (hName == "F") { 
				Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
				skinRend.material = assetsList.Skin_Materials [skinTy];
			}
		}


	}

	void OnValidate ()
	{
		//code for In Editor customize
		hairC = (int)hairCol;
		eyeC = (int)eyeCol;
		glassesT = glasses;
		hairT = (int)hair;
		faceT = (int)faceType;
        //skinT = (int)skinType;
        skinT = (int)faceType;
        coatC = (int)coatCol;

		tieT = tie;

		tieC = (int)tieCol;
		pantsC = (int)pantsCol;
		shoesC = (int)shoesCol;
		maskT = (bool)mask;
		stethoT = (bool)stethoscope;


		charCustomize (faceT, skinT, eyeC, glassesT, hairT, hairC, tieT, tieC, pantsC, shoesC, coatC, maskT, stethoT);

	}

}
