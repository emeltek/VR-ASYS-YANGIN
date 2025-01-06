using DG.Tweening;
using UnityEngine;
public class FireSystem : MonoBehaviour
{
    private ParticleSystem ps;
    public GameObject flameTarget;
    public ParticleSystem sparks;
    public ParticleSystem smoke;
    public AudioSource fireSound;
    public float sizeIncValue = 0.01f;
 
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        transform.DOMove(flameTarget.transform.position, 120);
    }

    void Update()
    {
        var shape = ps.shape;
        var emission = ps.emission;
        var main = ps.main;
        var sparksEmission = sparks.emission;
        var smokeMain = smoke.main;

        // Boyut artýþý (maksimum deðer ile sýnýrla)
        if (sizeIncValue < 5)
        {
            sizeIncValue += 0.1f * Time.deltaTime;
            main.startSize = sizeIncValue;
        }

        // Sparks ve smoke artýþýný sýnýrla
        float targetSparksRate = 50f; // Sabit hedef deðeri
        sparksEmission.rateOverTime = Mathf.Lerp(sparksEmission.rateOverTime.constant, targetSparksRate, Time.deltaTime);

        float targetSmokeSize = 3f; // Sabit hedef deðer
        smokeMain.startSize = Mathf.Lerp(smokeMain.startSize.constant, targetSmokeSize, Time.deltaTime * 0.5f);

        // Þekil yarýçapý artýþý (maksimum deðer ile sýnýrla)
        float targetShapeRadius = 5f;
        shape.radius = Mathf.Lerp(shape.radius, targetShapeRadius, Time.deltaTime * 0.1f);

        // Emission hýzýný sýnýrla
        float targetEmissionRate = 100f; // Sabit hedef deðer
        emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, targetEmissionRate, Time.deltaTime * 0.1f);
    }


}
