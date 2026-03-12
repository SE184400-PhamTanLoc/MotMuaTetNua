using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorExit : NPCBase
{
    public string sceneName;
    public GameObject pressText;

    protected override void Start()
    {
        base.Start();
        tenNPC = "ngoài sân";
        hanhDongTuongTac = "đi ra";
        quayVePhiaPlayer = false;

        if (pressText != null)
            pressText.SetActive(false);
    }

    protected override void OnTuongTac()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        KetThucTuongTac();
    }

    void Update() 
    { 
        base.Update(); 
        if (pressText != null && pressText.activeSelf) pressText.SetActive(false); 
    }
}