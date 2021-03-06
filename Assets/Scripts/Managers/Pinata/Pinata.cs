using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class Pinata : MonoBehaviour
{
    [SerializeField] private PinataAsset pinataData;
    [SerializeField] private SpriteRenderer pinataImage;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private string debugSize;
    [SerializeField] private GameObject pinataExplosion;

    private List<PlantGot> showPlants = new List<PlantGot>();
    private PinataSize selectedCategory;
    private int squishes, currentSquishes;
    private int totalGivenSeeds;
    private XRGrabInteractable grab;
    private Vector3 startPos;
    private bool returnToPos, squishCooldown, canGrab;
    private SpriteRenderer sr;
    private Collider coll;
    private Transform objTo;
    private Animator anim;

    void Awake()
    {
        grab = transform.GetChild(0).GetComponent<XRGrabInteractable>();
        objTo = transform.GetChild(0).transform;
        coll = objTo.GetComponent<Collider>();
        anim = GetComponent<Animator>();
        sr = objTo.GetComponent<SpriteRenderer>();

        worldCanvas = transform.GetChild(1).GetComponent<Canvas>();
        worldCanvas.worldCamera = GameManager.instance.GetMainCamera();
        startPos = objTo.position;
        StartCoroutine(InitialAnimation());
    }

    [System.Obsolete]
    void Start()
    {
        grab.onSelectEnter.AddListener(SendPinataToPlayer);
        grab.onSelectExit.AddListener(UnSendPinataToPlayer);
    }

    void Update()
    {   
        if (canGrab)
        {
            if (!grab.isSelected && Vector3.Distance(objTo.position, startPos) > 0.1f)
                returnToPos = true;

            if (returnToPos)
                GetBackPos();
        }
    }

    void SetRewardPlants()
    {
        int unlockedPlants = 0;

        foreach (PlantAsset pa in pinataData.plantsThatCanAppear)
            if (SeedDatabase.instance.PlayerOwnsPlant(pa))
                unlockedPlants++;

        if (unlockedPlants >= pinataData.minUnlockedPlantsToUse)
        {
            int plantsToShow = Random.Range(selectedCategory.plantsToAppearRange.x, selectedCategory.plantsToAppearRange.y);
            List<PlantGot> notSelectedPlants = new List<PlantGot>();

            for (int i = 0; i < pinataData.plantsThatCanAppear.Length; i++)
            {
                notSelectedPlants.Add(new PlantGot(pinataData.plantsThatCanAppear[i], 
                    Random.Range(selectedCategory.seedsToGiveRange.x, selectedCategory.seedsToGiveRange.y)));
            }
            
            for (int i = 0; i < plantsToShow; i++)
            {
                int randomPlant = Random.Range(0, notSelectedPlants.Count);
                showPlants.Add(notSelectedPlants[randomPlant]);
                notSelectedPlants.Remove(notSelectedPlants[randomPlant]);
            }
        }
    }

    public void CreateRewards()
    {
        for (int i = 0; i < showPlants.Count; i++)
        {
            GameObject prp = Instantiate(UIManager.instance.pinataRewardPanel, UIManager.instance.pinataRewardPanel.transform.position, Quaternion.identity, UIManager.instance.pinataGridPanel);
            prp.transform.GetChild(0).GetComponent<Text>().text = showPlants[i].plant.plantName;
            prp.transform.GetChild(1).GetComponent<Text>().text = showPlants[i].gotSeeds.ToString() + " Seedpackets";
            prp.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            prp.transform.localPosition = new Vector3(prp.transform.position.x, prp.transform.position.y, 0f);
            SeedDatabase.instance.BuyPlant(showPlants[i].plant, showPlants[i].gotSeeds);
            totalGivenSeeds += showPlants[i].gotSeeds;
        }

        UIManager.instance.pinataRewardText.text = "You got " + totalGivenSeeds + " seeds!";
    }

    public void SetPinataSize(string size)
    {
        pinataImage.sprite = pinataData.pinataSprite;

        switch (size)
        {
            case "s": selectedCategory = pinataData.sizes[0]; break;
            case "m": selectedCategory = pinataData.sizes[1]; break;
            case "l": selectedCategory = pinataData.sizes[2]; break;
            case "xl": selectedCategory = pinataData.sizes[3]; break;
        }

        for (int i = 0; i < pinataData.sizes.Length; i++)
        {   
            if (pinataData.sizes[i].pinataSize == selectedCategory.pinataSize)
            {
                squishes = Random.Range(pinataData.sizes[i].squishesRange.x, pinataData.sizes[i].squishesRange.y);
                break;
            }
        }

        SetRewardPlants();
    }

    public void Squish()
    {   
        if (!squishCooldown)
        {
            if (currentSquishes >= squishes)
            {
                coll.enabled = false;
                grab.enabled = false;
                sr.enabled = false;
                Player.instance.RecievePinata(null);
                SoundEffectsManager.instance.PlaySoundEffectNC("explosion");
                SoundEffectsManager.instance.PlaySoundEffectNC("prize");
                Instantiate(pinataExplosion, objTo.transform.position, Quaternion.identity);
                CreateRewards();
            }

            else
            {
                currentSquishes++;
                SoundEffectsManager.instance.PlaySoundEffectNC("coffee");

                if (GameHelper.GetRandomBool()) anim.SetTrigger("squishA");
                else anim.SetTrigger("squishB");
            }
        }
    }

    IEnumerator SquishCooldown()
    {
        squishCooldown = true;
        yield return new WaitForSeconds(.15f);
        squishCooldown = false;
    }
    
    IEnumerator InitialAnimation()
    {
        yield return new WaitForSeconds(0.25f);
        canGrab = true;
    }

    void SendPinataToPlayer(XRBaseInteractor inter)
    {
        Player.instance.RecievePinata(this);
    }

    void UnSendPinataToPlayer(XRBaseInteractor inter)
    {
        Player.instance.RecievePinata(null);
    }

    void GetBackPos()
    {
        objTo.position = Vector3.Lerp(objTo.position, startPos, 2.5f * Time.deltaTime);
        coll.enabled = false;

        if (Vector3.Distance(objTo.position, startPos) <= 0.01f)
        {
            objTo.position = startPos;
            returnToPos = false;

            if (currentSquishes < squishes)
                coll.enabled = true;
        }
    }
}

public class PlantGot
{
    public PlantAsset plant;
    public int gotSeeds;

    public PlantGot(PlantAsset plant, int gotSeeds)
    {
        this.plant = plant;
        this.gotSeeds = gotSeeds;
    }
}
