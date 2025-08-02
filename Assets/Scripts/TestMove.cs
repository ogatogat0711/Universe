using System;
using TMPro;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    public GameObject testObject;
    public TMP_InputField xField;
    public TMP_InputField yField;
    public TMP_InputField zField;
    public TMP_Text errorText;
    private Vector3 _inputVector;
    private bool _needToRotate;
    private float _vertical;
    private float _horizontal;
    public Camera mainCamera;
    private bool _didOnceSendMessage;
    public ParticleSystem leftSpark, rightSpark;

    void Start()
    {
        _inputVector = testObject.transform.position;
        _needToRotate = false;
        _vertical = 0f;
        _horizontal = 0f;
        _didOnceSendMessage = false;
        
        leftSpark.Stop();
        rightSpark.Stop();
    }
    void Update()
    {
        Debug.DrawRay(testObject.transform.position, testObject.transform.forward * 10, Color.blue);
        Debug.DrawRay(testObject.transform.position, _inputVector.normalized * 10, Color.red);
        
        _vertical = Input.GetAxis("Vertical");
        _horizontal = Input.GetAxis("Horizontal");

        if (_vertical != 0f || _horizontal != 0f)
        {
            _inputVector = mainCamera.transform.forward;//向かう方向
            
        }

        if (_needToRotate)
        {
            _didOnceSendMessage = false;
            
            Quaternion nextRotation = Quaternion.RotateTowards(testObject.transform.rotation,
                Quaternion.LookRotation(_inputVector), 50f * Time.deltaTime);
            
            testObject.transform.rotation = nextRotation;
        }

        _needToRotate = CheckRotation(_inputVector);

        if (!_didOnceSendMessage && !_needToRotate)
        {
            Debug.Log("Ready to Go Forward");
            _didOnceSendMessage = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            leftSpark.Play();
            rightSpark.Play();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            leftSpark.Stop();
            rightSpark.Stop();
        }
    }

    public void OnClick()
    {
        string inputX = xField.text;
        string inputY = yField.text;
        string inputZ = zField.text;
        
        if(float.TryParse(inputX, out float x) && float.TryParse(inputY, out float y) && float.TryParse(inputZ, out float z))
        {
            _inputVector = new Vector3(x, y, z);
            Debug.Log(_inputVector);
            
            errorText.text = ""; // エラーメッセージをクリア
            
            if (x == 0 && y == 0 && z == 0)
            {
                errorText.text = "Zero vector is prohibited.";
            }
            
            
            
        }
        else
        {
            errorText.text = "Invalid input.\n Please enter valid numbers.";
        }
    }
    
    private bool CheckRotation(Vector3 headingDirection)
    {
        // 回転が必要かどうかを判定する
        
        float angle = Vector3.Angle(testObject.transform.forward, headingDirection);
   
        //Debug.Log("angle: " + angle);
        return angle > 0.5f; // 2度より大きいなら回転が必要とする
    }
}
