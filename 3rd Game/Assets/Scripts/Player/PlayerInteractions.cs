using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerInteractions : MonoBehaviour
{
    public static int StarsNum { get; private set; }
    public static List<int> IndexsOfObtainedStars { get; private set; }
    public static bool Dead , Win;

    public Material StartCol;
    public GameObject DeathPartEff, RingPassPartEff;    
    [HideInInspector] public float Origin;
    [Tooltip("How much the player needs to fall from his original platform to Die")]
    public float FallLimit;
    [Tooltip("The quantity of speed to add to my original speed after the speed boost lv1")]
    public float BoostValueLv1;
    [Tooltip("The quantity of speed to add to my original speed after the speed boost lv2")]
    public float BoostValueLv2;
    [Tooltip("The Duraction of the Speed Boost (In Seconds)")]
    public float SpeedBoostTime;
    public List<TrailProperties> Trails;

    [Header("Events")]
    public UnityEvent<int> ShowStar;
    public UnityEvent Won;
    public UnityEvent Lost;

    private LayerMask ColorSwitch , ColorObst , FinishLine , SpeedBoost , StarLayer;
    private MeshRenderer Mat;
    private PlayerMovement Pm;
    private EffectsBehavior Effs;
    private bool AlreadyIn;
    private GameObject Star; //The Star that will Disappear after the player gets it

    void Start()
    {
        AlreadyIn = false;
        StarsNum = 0;
        Dead = false;
        Win = false;
        Origin = transform.position.y;
        Pm = GetComponent<PlayerMovement>();
        Mat = GetComponent<MeshRenderer>();
        Effs = Camera.main.GetComponent<EffectsBehavior>();

        ColorSwitch = LayerMask.NameToLayer("Color Switch");
        ColorObst = LayerMask.NameToLayer("Color Obst");
        FinishLine = LayerMask.NameToLayer("Finish Line");
        SpeedBoost = LayerMask.NameToLayer("Speed Boost");
        StarLayer = LayerMask.NameToLayer("Star");

        Mat.material.color = StartCol.color;
        ChangeTrail();

        RingPassPartEff = Instantiate(RingPassPartEff, transform.position, new Quaternion());
        RingPassPartEff.SetActive(false);

        //Save System Stuff
        int CurLv = SceneManager.GetActiveScene().buildIndex;

        IndexsOfObtainedStars = PlayerData.LvXStars[CurLv] > 1
                       ? PlayerData.CollectedStarsIndex[CurLv - 1] : new List<int>();

        StarsNum = PlayerData.LvXStars[CurLv];

        for(int i = 0; i < StarsNum; i++)
        {
            ShowStar.Invoke(i+1);
        }
    }

    void Update()
    {
        if (!ScreensEventHandlers.IsPaused)
        {
            if (Origin - transform.position.y >= FallLimit)
            {
                Die();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == ColorSwitch)
        {
            Mat.material.color = other.GetComponent<MeshRenderer>().material.color;

            ChangeTrail();

            AudioManager.AudMan.Play("Color Switch" , true);
        }
        else if (other.gameObject.layer == ColorObst)
        {
            DeathCheck(other);
        }
        else if (other.gameObject.layer == SpeedBoost)
        {
            float BoostVal = 0, BoostTimeVal = SpeedBoostTime;
            int BoostLv = 1;
            bool TakeInput = true;
            BoostProperties Bp = other.GetComponent<BoostProperties>();

            //This can be considered the Boost Presets
            if (other.tag == "Boost lv 1")
            {
                BoostVal = BoostValueLv1;
                BoostLv = 1;
            }
            else if (other.tag == "Boost lv 2")
            {
                BoostVal = BoostValueLv2;
                TakeInput = false;
                BoostLv = 2;
            }

            if (Bp.OverideBoostVal)
            {
                BoostVal = Bp.OveridedBoostVal;
            }

            if (Bp.OverideBoostTime)
            {
                BoostTimeVal = Bp.OveridedBoostTime;
            }

            StartCoroutine(Pm.SpeedUp(BoostVal, BoostTimeVal, TakeInput, BoostLv));

        }
        else if (other.tag == "Cannon Stuff")
        {
            other.transform.parent.GetComponent<CannonsObs>().DestCan(other.transform);
        }
        else if (other.gameObject.layer == StarLayer)
        {
            if (!AlreadyIn)
            {
                AudioManager.AudMan.Play("Star");

                AlreadyIn = true;

                //Increase Stars
                StarsNum++;
                ShowStar.Invoke(StarsNum);

                //Deal With The Star
                other.GetComponent<Collider>().enabled = false;

                var index = other.transform.parent.GetSiblingIndex();
                IndexsOfObtainedStars.Add(index);
                Debug.Log("I Got the star at the index = " + index);


                other.GetComponent<Animator>().SetTrigger("Disappear");

                StartCoroutine(DisbaleStar(other.transform.parent.gameObject , .5f));
            }
        }

        AlreadyIn = false;
    }   

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == ColorObst)
        {
            DeathCheck(collision.collider);
        }
        else if (collision.gameObject.layer == FinishLine)
        {
            if(IndexsOfObtainedStars.Count == StarsNum)
            {
                StarsNum++;
                ShowStar.Invoke(StarsNum);
            }

            //Because Disbaling The Star Won't Work when I stop all coroutines
            if (Star != null)
            {
                Star.SetActive(false); 
            }
           
            StopAllCoroutines();
            StartCoroutine(Pm.Stop());            

            Win = true;
            AudioManager.AudMan.StopAll();
            if (PlayerData.Vibrations)
            {
                Vibration.VibrateNope();
            }

            Won.Invoke();
        }
        else if (collision.transform.tag == "Obstacle")
        {            
            Die();
        }
    }

    #region Coroutines (DisbaleStar, RingPassBehavior)

    IEnumerator DisbaleStar(GameObject star, float Time)
    {
        this.Star = star;

        yield return new WaitForSeconds(Time);

        star.SetActive(false);
    }

    IEnumerator RingPassBehavior(Vector3 EffPos)
    {
        yield return new WaitUntil(() => transform.position.z - EffPos.z > .5f);

        AudioManager.AudMan.Play("Ring Passed", true);

        RingPassPartEff.transform.position = EffPos;
        RingPassPartEff.SetActive(true);

        yield return new WaitForSeconds(1);

        RingPassPartEff.SetActive(false);
    }

    #endregion

    #region Death Related

    void DeathCheck(Collider col)
    {
        Material mat = null;        

        if(col.gameObject.TryGetComponent(out MeshRenderer mesh))
        {
            //If the Collided Object have Mesh Renderer Then I will check color Directly
            mat = mesh.material;
        }
        else
        { 
            //If the Collided Object doesn't have Mesh Renderer
            //Then I will get the color throught the IColliderColor Interface
            mat = col.gameObject.GetComponent<IColliderColor>().GetColor();
        }

        if (!CompColorsRGB(Mat.material.color, mat.color))
        {          
            Die();
        }
        else
        {
            //If the player doesn't Die then He will do stuff based on which Obstacales he passed
            if (col.tag == "Ring")
            {
                StartCoroutine(RingPassBehavior(transform.position));
            }
        }     
    }

    bool CompColorsRGB(Color color1, Color color2)
    {
        //Creating a Comparisent that only takes into account the RGB Values
        return color1.r == color2.r && color1.g == color2.g && color1.b == color2.b;
    }

    void Die()
    {
        //To not Go In 2 Times in Row
        if (!Dead)
        {
            Debug.Log("You Died !");

            Dead = true;

            AudioManager.AudMan.StopAll();
            AudioManager.AudMan.Play("Lost");
            AudioManager.AudMan.Play("Died");            

            StopAllCoroutines();
            Pm.StopAllCoroutines();
            Pm.CancelAllEffects();
            CameraShaker.Instance.ShakeOnce(7, 5, .1f, .5f);

            ParticleSystemRenderer PS = Instantiate(DeathPartEff, transform.position, new Quaternion()).GetComponent<ParticleSystemRenderer>();
            PS.transform.Rotate(Vector3.right * -90);
            PS.material = Mat.material;

            if (PlayerData.Vibrations)
            {
                Vibration.VibratePop();
            }            

            Lost.Invoke();

            Destroy(gameObject);
        }       
    }

    #endregion
      
    #region Trail Related

    void ChangeTrail()
    {
        Material mat = Effs.trailRendrer.material;

        TrailProperties trail = Trails.Find(trail => trail.ConcernedMat.color == Mat.material.color);

        SetTrail(trail, mat);        
    }
 
    void SetTrail(TrailProperties trail, Material mat)
    {
        //Setting Particles Color
        Effs.TrailParticles.SetVector4("Particles Color", Mat.material.color);

        //Setting the trail Material colors
        mat.SetColor("_Color1", trail.Color1 * Mathf.Pow(2 , trail.Intensity1));
        mat.SetColor("_Color2", trail.Color2 * Mathf.Pow(2, trail.Intensity2));

        //Setting Trail Rendrer Valus
        Effs.trailRendrer.startWidth = trail.width;
        Effs.trailRendrer.endWidth = trail.width;
        Effs.trailRendrer.time = trail.Time;
        
    }

    #endregion
}
