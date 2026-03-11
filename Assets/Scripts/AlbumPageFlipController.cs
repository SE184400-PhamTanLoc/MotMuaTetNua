using UnityEngine;

/// <summary>
/// Điều khiển lật lần lượt tờ 2 → tờ 6 của album 3D.
/// Tờ 1 (và bìa) điều khiển riêng; tờ 7 không lật, tự hiện khi tờ 6 lật.
/// Gán 5 pivot (Page2…Page6), mỗi pivot có AlbumCoverFlip.
/// </summary>
public class AlbumPageFlipController : MonoBehaviour
{
    [Header("Tờ 2 → 6 (lật lần lượt)")]
    [Tooltip("Pivot tờ 2, 3, 4, 5, 6 — mỗi object cần có AlbumCoverFlip. Thứ tự đúng: [0]=tờ 2, [1]=tờ 3, ...")]
    public AlbumCoverFlip[] pageFlips = new AlbumCoverFlip[5];
    
    [Header("Audio")]
    public AudioSource pageAudioSource;
    public AudioClip pageFlipClip;
    [Tooltip("Bật để không phát âm ở lần lật đầu tiên (thường dùng cho cover).")]
    public bool skipFirstFlipSound = true;

    /// <summary>
    /// Chỉ số tờ đang chờ lật tiếp: 0 = chưa lật tờ 2, 1 = đã lật tờ 2 (tiếp theo là tờ 3), ..., 5 = đã lật hết tờ 6.
    /// </summary>
    private int _currentIndex;

    /// <summary>
    /// Đã lật hết tờ 2→6 chưa.
    /// </summary>
    public bool AllSheetsFlipped => _currentIndex >= 5;

    /// <summary>
    /// Có thể lật tờ tiếp theo (tờ 2→6) không.
    /// </summary>
    public bool CanFlipNext => _currentIndex < 5 && pageFlips != null && _currentIndex < pageFlips.Length && pageFlips[_currentIndex] != null;

    void Start()
    {
        _currentIndex = 0;
        if (pageAudioSource == null)
        {
            pageAudioSource = GetComponent<AudioSource>();
        }
        EnsureSfxChannelVolume(pageAudioSource);
        // Đảm bảo tất cả tờ 2-6 ban đầu đóng
        if (pageFlips != null)
        {
            for (int i = 0; i < pageFlips.Length; i++)
            {
                if (pageFlips[i] != null)
                    pageFlips[i].Close();
            }
        }
    }

    /// <summary>
    /// Lật tờ tiếp theo trong dãy (tờ 2 → 3 → 4 → 5 → 6). Gọi mỗi lần nhấn E (hoặc nút).
    /// </summary>
    /// <returns>True nếu đã gửi lệnh lật tờ tiếp, false nếu không còn tờ nào để lật.</returns>
    public bool FlipNextPage()
    {
        if (!CanFlipNext)
            return false;

        int flipIndex = _currentIndex;
        pageFlips[_currentIndex].Open();
        if (!(skipFirstFlipSound && flipIndex == 0))
        {
            PlayPageFlipClip();
        }
        _currentIndex++;
        return true;
    }

    /// <summary>
    /// Reset về trạng thái chưa lật tờ nào (đóng hết tờ 2-6). Dùng khi mở lại album.
    /// </summary>
    public void ResetPages()
    {
        _currentIndex = 0;
        if (pageFlips != null)
        {
            for (int i = 0; i < pageFlips.Length; i++)
            {
                if (pageFlips[i] != null)
                    pageFlips[i].Close();
            }
        }
    }
    
    private void PlayPageFlipClip()
    {
        if (pageFlipClip == null)
        {
            return;
        }

        if (pageAudioSource != null)
        {
            EnsureSfxChannelVolume(pageAudioSource);
            pageAudioSource.PlayOneShot(pageFlipClip);
            return;
        }

        if (Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(pageFlipClip, Camera.main.transform.position);
        }
    }

    private void EnsureSfxChannelVolume(AudioSource source)
    {
        if (source == null) return;

        AudioChannelVolume channelVolume = source.GetComponent<AudioChannelVolume>();
        if (channelVolume == null)
        {
            channelVolume = source.gameObject.AddComponent<AudioChannelVolume>();
        }

        channelVolume.channel = AudioChannelType.Sfx;
        channelVolume.useAudioSourceVolumeAsBaseOnAwake = false;
        channelVolume.baseVolume = source.volume;
        channelVolume.ApplyCurrentVolume();
    }
}
