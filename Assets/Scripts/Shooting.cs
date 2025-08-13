using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class Shooting : MonoBehaviour
{
   public GameObject shotPrefab;
   public Probe probe;
   public CinemachineVirtualCameraBase fpsCamera;
   public Camera mainCamera;
   public Transform shootRoot; // 射撃のルート位置
   public Transform shotParent; // 射撃の親オブジェクト
   private float _offsetForRoot = 0.75f;
   private float _shootingTimer;
   public TMP_Text shootingInfo;
   private ShotData _shotData;

   void Start()
   {
      _shotData = probe.shotDataList.FindShotDataById(probe.shotID);
      shotPrefab = _shotData.shotPrefab;
      _shootingTimer = 0f;
      shootingInfo.text = ": 射撃(消費燃料: " + _shotData.fuelConsumptionOfShot + ")";
   }
   
   void FixedUpdate()
   {
      Vector3 forward = fpsCamera.transform.forward;
      shootRoot.position = probe.transform.position + forward * _offsetForRoot; // 射撃位置をProbeの位置から前方に調整
      shootRoot.rotation = Quaternion.LookRotation(forward); // 射撃位置の向きをカメラの前方に合わせる
      
      if (fpsCamera.IsLive && Input.GetMouseButton(0)) // 左クリックで射撃
      {
         _shootingTimer += Time.fixedDeltaTime;

         if (_shootingTimer >= _shotData.shotInterval) // 射撃間隔を確認
         {
            Shoot();
            _shootingTimer = 0f; // タイマーリセット
         }
      }
      else
      {
         _shootingTimer = _shotData.shotInterval; //すぐに打てるようにしておく
      }
   }

   private void Shoot()
   {
      Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

      Ray ray = mainCamera.ScreenPointToRay(screenCenter); // 画面の中心からレイを発射
      Vector3 direction = ray.direction.normalized;


      Quaternion rotation =
         Quaternion.LookRotation(direction) * Quaternion.AngleAxis(90, Vector3.right); //shotの向きをカメラの方向に合わせる
      GameObject shotObject = Instantiate(shotPrefab, shootRoot.position, rotation);
      Shot shot = shotObject.AddComponent<Shot>();
      shot.transform.SetParent(shotParent); // 射撃の親オブジェクトに設定
      shot.InitShot(_shotData);
      Rigidbody rb = shot.GetComponent<Rigidbody>();
      rb.AddForce(direction * shot.speed, ForceMode.Impulse);
      probe.fuel-= shot.fuelConsumptionRatioOfShot; //燃料消費

   }
}
