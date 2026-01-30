using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using static InteractiveTradeWallDataSO;

public class HorizontalSlideshow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [Header("UI Setup")]
    public RectTransform container;   // Parent that holds all slides

    public GameObject slidePrefab;    // Prefab with Image
    public List<BookmarkImage> slides = new List<BookmarkImage>();

    [Header("Settings")]
    public float slideSpeed = 0.3f;      // Snap speed

    public float swipeThreshold = 0.2f;  // % of screen width to count as swipe

    private int currentIndex = 0;
    private Vector2 dragStartPos;
    private Vector2 containerStartPos;
    private float slideWidth;

    [Header("Dots")]
    public TMP_Text picInfoText;

    public Text picInfoTxt;
    public Transform SliderDotsParent;
    public GameObject SliderDotsPrefab;
    public List<Toggle> SliderDots;

    private void Start() {
        //PopulateSlides();
        //PopulateDots();
        //if (slides.Count > 0 && container.childCount > 0)
        //    slideWidth = ((RectTransform)container.GetChild(0)).rect.width;

        //SnapToSlide(false);
    }

    public void SetupSlides(List<BookmarkImage> _slides) {
        //Debug.Log("SetupSlides" + _slides.Count + ">" + slides[0].title);
        slides = _slides;
        currentIndex = 0;
        PopulateSlides();
        PopulateDots();
        if (slides.Count > 0 && container.childCount > 0)
            slideWidth = ((RectTransform)container.GetChild(0)).rect.width;

        SnapToSlide(false);
    }

    private void PopulateSlides() {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        foreach (var _slide in slides) {
            GameObject slide = Instantiate(slidePrefab, container);
            // BookController.instance.LoadImageFromURL(_slide.imagePath, slide.GetComponent<RawImage>());

            //[OLD]
            //BookController.instance.LoadTextureFromResources(_slide.imagePath, slide.GetComponent<RawImage>());
            //[NEW]
            BookController.instance.LoadTextureFromResources(_slide.imagePath,(sprite) => { 
                slide.GetComponent<RawImage>().texture = sprite.texture;
            });
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        dragStartPos = eventData.position;
        containerStartPos = container.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 delta = eventData.position - dragStartPos;
        container.anchoredPosition = containerStartPos + new Vector2(delta.x, 0); // Move while dragging
    }

    public void OnEndDrag(PointerEventData eventData) {
        float dragDistance = eventData.position.x - dragStartPos.x;
        float dragPercent = Mathf.Abs(dragDistance) / Screen.width;

        if (dragPercent > swipeThreshold) {
            if (dragDistance < 0 && currentIndex < slides.Count - 1)
                currentIndex++; // Swipe left → next
            else if (dragDistance > 0 && currentIndex > 0)
                currentIndex--; // Swipe right → previous
        }

        SnapToSlide(true);
    }

    private void SnapToSlide(bool animate) {
        float targetX = -currentIndex * slideWidth;
        if (animate)
            StartCoroutine(SmoothSlide(targetX));
        else
            container.anchoredPosition = new Vector2(targetX, 0);

        UpdateMetaData();
    }

    private System.Collections.IEnumerator SmoothSlide(float targetX) {
        Vector2 startPos = container.anchoredPosition;
        Vector2 endPos = new Vector2(targetX, 0);

        float t = 0;
        while (t < 1f) {
            t += Time.deltaTime / slideSpeed;
            container.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        container.anchoredPosition = endPos;
    }

    // 🔹 Go to Next Slide
    public void NextSlide() {
        if (currentIndex < slides.Count - 1) {
            currentIndex++;
            SnapToSlide(true);
        }
    }

    // 🔹 Go to Previous Slide
    public void PrevSlide() {
        if (currentIndex > 0) {
            currentIndex--;
            SnapToSlide(true);
        }
    }

    public void UpdateMetaData() {
        //picInfoText.text = slides[currentIndex].title + "\n" + slides[currentIndex].subtitle;
        picInfoTxt.text = slides[currentIndex].title + "\n" + slides[currentIndex].subtitle;
        SliderDots[currentIndex].isOn = true;
    }

    private void PopulateDots() {
        // Clear old slides
        foreach (var t in SliderDots)
            Destroy(t.gameObject);

        SliderDots.Clear();

        ToggleGroup tg = SliderDotsParent.GetComponent<ToggleGroup>();
        // Create new slides
        foreach (var pic_data in slides) {
            GameObject _dot = Instantiate(SliderDotsPrefab, SliderDotsParent);
            Toggle dotToggle = _dot.GetComponent<Toggle>();
            dotToggle.group = tg;
            SliderDots.Add(dotToggle);
        }
    }
}