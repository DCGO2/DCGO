using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class UIDebugTheme
{
  private static readonly Color BackgroundColor = new Color(0.11f, 0.15f, 0.25f, 0.90f);
  private static readonly Color PanelColor = new Color(0.17f, 0.24f, 0.31f, 0.85f);
  private static readonly Color ButtonColor = new Color(0.16f, 0.50f, 0.73f, 1.00f);
  private static readonly Color ToggleOnColor = new Color(0.53f, 0.81f, 0.98f, 1.00f); // light blue
  private static readonly Color ToggleOffColor = new Color(0.10f, 0.22f, 0.40f, 1.00f); // dark blue  
  private static readonly Color RadioOnColor = new Color(0.15f, 0.85f, 0.55f, 1.00f); // bright green — distinct from all blues  
  private static readonly Color LeafColor = new Color(0.36f, 0.43f, 0.49f, 1.00f);

  private static readonly Dictionary<int, Color> _originals = new Dictionary<int, Color>();
  private static readonly Dictionary<Toggle, bool> _lastToggleStates = new Dictionary<Toggle, bool>();
  private static readonly List<GameObject> _addedLabels = new List<GameObject>();
  private static readonly Dictionary<int, ColorBlock> _originalBlocks = new Dictionary<int, ColorBlock>();
  private static readonly Dictionary<int, Selectable.Transition> _originalTransitions = new Dictionary<int, Selectable.Transition>();
  private static readonly Dictionary<int, Material> _originalMaterials = new Dictionary<int, Material>();

  private static double _nextRescanTime = 0;

  // ColorBlock for regular interactables (buttons etc.). normalColor=white leaves
  // image.color unmodified in normal state; >1 values lighten on hover/select,
  // <1 darkens on press.
  private static readonly ColorBlock ThemedBlock = new ColorBlock
  {
    normalColor = Color.white,
    highlightedColor = new Color(1.20f, 1.20f, 1.20f, 1.0f),
    pressedColor = new Color(0.65f, 0.65f, 0.65f, 1.0f),
    selectedColor = new Color(1.15f, 1.15f, 1.15f, 1.0f),
    disabledColor = new Color(0.50f, 0.50f, 0.50f, 0.5f),
    colorMultiplier = 1f,
    fadeDuration = 0.1f,
  };

  // ColorBlock for input fields. normalColor is light gray so the background is
  // visible and differs from pure-white text, and selectedColor shows a blue tint
  // when the field has focus so the user can tell it is active.
  private static readonly ColorBlock InputFieldBlock = new ColorBlock
  {
    normalColor = new Color(0.82f, 0.82f, 0.82f, 1.00f),
    highlightedColor = new Color(0.90f, 0.90f, 0.90f, 1.00f),
    pressedColor = new Color(0.60f, 0.60f, 0.60f, 1.00f),
    selectedColor = new Color(0.65f, 0.83f, 0.98f, 1.00f), // light blue when focused
    disabledColor = new Color(0.50f, 0.50f, 0.50f, 0.50f),
    colorMultiplier = 1f,
    fadeDuration = 0.1f,
  };

  static UIDebugTheme()
  {
    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
  }

  static void OnPlayModeStateChanged(PlayModeStateChange state)
  {
    switch (state)
    {
      case PlayModeStateChange.EnteredPlayMode:
        ApplyTheme();
        EditorApplication.update += PollToggleStates;
        break;

      case PlayModeStateChange.ExitingPlayMode:
        EditorApplication.update -= PollToggleStates;
        CleanupToggleTracking();
        _originals.Clear();
        _originalBlocks.Clear();
        _originalTransitions.Clear();
        _originalMaterials.Clear();
        break;
    }
  }

  static void ApplyTheme()
  {
    _originals.Clear();

    var images = Object.FindObjectsOfType<Image>(true);
    int count = 0;

    foreach (var image in images)
    {
      if (!PassesSpriteFilter(image))
      {
        continue;
      }

      int id = image.GetInstanceID();
      _originals[id] = image.color;
      image.color = ClassifyColor(image);
      count++;
    }

    SetupToggleTracking();
    AddIconButtonLabels();
    AddMissingAssetLabels();
    ApplySelectableColors();

    Debug.Log($"[UIDebugTheme] Applied debug colours to {count} Image component{(count == 1 ? "" : "s")} with missing sprites.");
  }

  // Returns true for Images that should receive debug theme colours:
  // sprite must be null AND color must be near-white (or have a direct text child
  // whose sprite was providing the label background).
  static bool PassesSpriteFilter(Image image)
  {
    if (image.sprite != null) { return false; }

    // Never theme images that live inside a CardImage hierarchy — those are
    // card art / key-card visuals and must keep their original colours.
    for (Transform t = image.transform; t != null; t = t.parent)
    {
      if (t.name == "CardImage" || t.name == "DetailCard") { return false; }
    }

    var c = image.color;
    bool isBlankWhite = c.a >= 0.5f && c.r >= 0.9f && c.g >= 0.9f && c.b >= 0.9f;
    if (isBlankWhite) { return true; }

    // Non-white but has a direct text child — the sprite was providing the label
    // background and must be replaced so text remains readable.
    for (int i = 0; i < image.transform.childCount; i++)
    {
      var child = image.transform.GetChild(i);
      if (child.GetComponent<Text>() != null || child.GetComponent<TextMeshProUGUI>() != null)
        return true;
    }
    return false;
  }

  static Color ClassifyColor(Image image)
  {
    var go = image.gameObject;

    if (go.GetComponent<Button>() != null)
    {
      return ButtonColor;
    }

    Toggle toggleSelf = go.GetComponent<Toggle>();
    Toggle toggleParent = go.transform.parent != null
        ? go.transform.parent.GetComponent<Toggle>()
        : null;
    Toggle owningToggle = toggleSelf ?? toggleParent;
    if (owningToggle != null && owningToggle.targetGraphic == image)
    {
      Color onColor = owningToggle.group != null ? RadioOnColor : ToggleOnColor;
      return owningToggle.isOn ? onColor : ToggleOffColor;
    }

    if (go.GetComponent<Scrollbar>() != null || go.GetComponent<Slider>() != null)
    {
      return ButtonColor;
    }

    var rt = image.rectTransform;

    if (rt.parent != null && rt.parent.GetComponent<Canvas>() != null)
    {
      return BackgroundColor;
    }

    bool isFullStretch =
        rt.anchorMin == Vector2.zero &&
        rt.anchorMax == Vector2.one &&
        rt.offsetMin.sqrMagnitude < 100f &&
        rt.offsetMax.sqrMagnitude < 100f;

    if (isFullStretch)
    {
      return BackgroundColor;
    }

    var childGraphics = go.GetComponentsInChildren<Graphic>(true);
    if (childGraphics.Length > 1)
    {
      // Distinguish containers (have child Image/RawImage) from labeled badges (text-only children).
      // Badges like player-number boxes have only a text child and should use ButtonColor.
      bool hasChildImage = false;
      foreach (var g in childGraphics)
      {
        if (g == image) { continue; }
        if (g is Image || g is RawImage) { hasChildImage = true; break; }
      }
      return hasChildImage ? PanelColor : ButtonColor;
    }

    return LeafColor;
  }

  static void SetupToggleTracking()
  {
    var toggles = Object.FindObjectsOfType<Toggle>(true);
    foreach (var toggle in toggles)
    {
      if (toggle.targetGraphic is not Image bg)
      {
        continue;
      }
      // For non-radio (non-group) toggles, skip if the targetGraphic has a valid
      // sprite — it has a real designed visual and we shouldn't clobber it.
      // Radio buttons (ToggleGroup members) are always processed so their state
      // color is enforced even when the sprite reference resolves to an object.
      if (bg.sprite != null && toggle.group == null)
      {
        continue;
      }
      if (_lastToggleStates.ContainsKey(toggle))
      {
        continue;
      }

      _lastToggleStates[toggle] = toggle.isOn;

      Color onCol = toggle.group != null ? RadioOnColor : ToggleOnColor;
      Color stateCol = toggle.isOn ? onCol : ToggleOffColor;

      // Force-color the targetGraphic even if ApplyTheme skipped it (e.g. the
      // Image had a non-white default color and no text child).
      int bgId = bg.GetInstanceID();
      if (!_originals.ContainsKey(bgId)) { _originals[bgId] = bg.color; }
      bg.color = stateCol;

      // Also color toggle.graphic (the checkmark image). Unity makes the checkmark
      // visible when isOn=true. If it got classified as BackgroundColor by ApplyTheme
      // it would overlay the indicator with the wrong color and mask our state color.
      if (toggle.graphic is Image checkmark && checkmark != bg)
      {
        int ckId = checkmark.GetInstanceID();
        if (!_originals.ContainsKey(ckId)) { _originals[ckId] = checkmark.color; }
        checkmark.color = stateCol;
      }

      // Radio buttons express state through color alone —
      // their sibling text already provides the label. Only non-grouped toggles
      // get an ON/OFF overlay.
      if (toggle.group == null)
      {
        SetToggleLabel(toggle, toggle.isOn);
      }
    }
  }

  static void AddIconButtonLabels()
  {
    var buttons = Object.FindObjectsOfType<Button>(true);
    foreach (var btn in buttons)
    {
      if (btn.targetGraphic is not Image bg)
      {
        continue;
      }
      if (!_originals.ContainsKey(bg.GetInstanceID()))
      {
        continue;
      }
      if (bg.transform.Find("UIDebugTheme_IconLabel") != null)
      {
        continue;
      }

      string label = GetIconLabel(btn, bg, bg.rectTransform);
      if (label == null)
      {
        continue;
      }

      if (btn.GetComponentsInChildren<Button>(true).Length > 1)
      {
        continue;
      }

      // Ensure settings buttons get a recognisable dark background.
      // Color the full subtree (button + all child Images) AND every ancestor
      // Image up to the Canvas, regardless of whether they were in _originals.
      if (label == "config")
      {
        foreach (var img in btn.GetComponentsInChildren<Image>(true))
        {
          int cid = img.GetInstanceID();
          if (!_originals.ContainsKey(cid)) { _originals[cid] = img.color; }
          img.color = ButtonColor;
        }
        for (Transform t = btn.transform.parent; t != null; t = t.parent)
        {
          if (t.GetComponent<Canvas>() != null) { break; }
          var parentImg = t.GetComponent<Image>();
          if (parentImg == null) { continue; }
          int pid = parentImg.GetInstanceID();
          if (!_originals.ContainsKey(pid)) { _originals[pid] = parentImg.color; }
          parentImg.color = ButtonColor;
        }
      }

      var labelGO = new GameObject("UIDebugTheme_IconLabel");
      labelGO.transform.SetParent(bg.transform, false);

      var lrt = labelGO.AddComponent<RectTransform>();
      lrt.anchorMin = Vector2.zero;
      lrt.anchorMax = Vector2.one;
      lrt.offsetMin = Vector2.zero;
      lrt.offsetMax = Vector2.zero;

      var tmp = labelGO.AddComponent<TextMeshProUGUI>();
      tmp.text = label;
      tmp.alignment = TextAlignmentOptions.Center;
      tmp.color = Color.white;
      tmp.fontStyle = FontStyles.Bold;
      tmp.enableAutoSizing = true;
      tmp.fontSizeMin = 8;
      tmp.fontSizeMax = 36;

      _addedLabels.Add(labelGO);
    }
  }

  // Builds a '>'-joined lowercase path string from root to the button's GO.
  static string GetButtonPath(Button btn)
  {
    var parts = new List<string>();
    for (Transform t = btn.transform; t != null; t = t.parent)
    {
      parts.Add(t.name.ToLowerInvariant());
    }
    parts.Reverse();
    return string.Join(">", parts);
  }

  // Returns the debug label for an icon-only button, or null to leave it unlabelled.
  // "config" is matched by a specific container name anywhere in the full hierarchy path
  //    (no TMP filter — the button may have internal text layers).
  // "X" is matched by name/anchor but only on buttons with no pre-existing text children
  //    so we don't overlay a label on top of real button text.
  static string GetIconLabel(Button btn, Image bg, RectTransform rt)
  {
    string path = GetButtonPath(btn);

    // Settings / option icon buttons — match by specific container name in the path.
    if (path.Contains("optionbutton"))
    {
      return "config";
    }

    // For X labels only operate on buttons with no pre-existing text children,
    // otherwise we'd overlay the label on real button text.
    bool hasText = bg.GetComponentInChildren<TextMeshProUGUI>() != null
                || bg.GetComponentInChildren<Text>() != null;
    if (hasText)
    {
      return null;
    }

    string selfName = btn.gameObject.name.ToLowerInvariant();
    string parentName = btn.transform.parent != null ? btn.transform.parent.name.ToLowerInvariant() : "";

    // Deck editor pagination — child GOs named "Left"/"Right" inside the EditDeck panel.
    if (path.Contains(">left>")) { return "<"; }
    if (path.Contains(">right>")) { return ">"; }

    // The EditDeck panel has no X close buttons, and many buttons anchored to
    // the top-right of their components (card pool pagination buttons, deck 
    // card +/- buttons), so suppress any "X" behavior.
    if (path.Contains(">editdeck>")) { return null; }

    // Close button by name (button GO or direct parent).
    if (selfName.Contains("close") || selfName.Contains("dismiss") ||
        parentName.Contains("close") || parentName.Contains("dismiss"))
    {
      return "X";
    }

    // Anchor-based fallback: top-right anchored icon-only buttons are likely close buttons.
    if (rt.anchorMin.x >= 0.5f && rt.anchorMin.y >= 0.5f)
    {
      return "X";
    }

    return null;
  }

  static void PollToggleStates()
  {
    if (EditorApplication.timeSinceStartup >= _nextRescanTime)
    {
      _nextRescanTime = EditorApplication.timeSinceStartup + 1.0;
      RescanNewImages();
    }

    // Enforce correct colors every frame — game code (Awake/Start) can reset image
    // colors after ApplyTheme runs, and state-change detection alone won't catch that.
    // We color every tracked image in the toggle's entire subtree (not just targetGraphic)
    // so that the checkmark or any other child image cannot cover the indicator with
    // a misclassified color.
    foreach (var kvp in _lastToggleStates)
    {
      var toggle = kvp.Key;
      if (toggle == null) { continue; }
      Color onColor = toggle.group != null ? RadioOnColor : ToggleOnColor;
      Color stateColor = toggle.isOn ? onColor : ToggleOffColor;
      foreach (var img in toggle.GetComponentsInChildren<Image>(true))
      {
        if (_originals.ContainsKey(img.GetInstanceID()) && img.color != stateColor)
          img.color = stateColor;
      }
    }

    // Detect state changes to update ON/OFF labels and the tracked-state dict.
    List<Toggle> changed = null;
    foreach (var kvp in _lastToggleStates)
    {
      if (kvp.Key == null || kvp.Key.isOn == kvp.Value)
      {
        continue;
      }
      changed ??= new List<Toggle>();
      changed.Add(kvp.Key);
    }

    if (changed == null)
    {
      return;
    }

    foreach (var toggle in changed)
    {
      _lastToggleStates[toggle] = toggle.isOn;
      if (toggle.group == null)
      {
        SetToggleLabel(toggle, toggle.isOn);
      }
    }
  }

  static void RescanNewImages()
  {
    // Prune destroyed toggles so the dict doesn't grow unboundedly in scenes
    // with dynamic UI (object pooling, Destroy calls, etc.).
    var deadToggles = new List<Toggle>();
    foreach (var toggle in _lastToggleStates.Keys)
      if (toggle == null) deadToggles.Add(toggle);
    foreach (var t in deadToggles) _lastToggleStates.Remove(t);

    var images = Object.FindObjectsOfType<Image>(true);
    int count = 0;

    foreach (var image in images)
    {
      int id = image.GetInstanceID();
      if (_originals.ContainsKey(id))
      {
        continue;
      }
      if (!PassesSpriteFilter(image))
      {
        continue;
      }

      _originals[id] = image.color;
      image.color = ClassifyColor(image);
      count++;
    }

    if (count > 0)
    {
      SetupToggleTracking();
      AddIconButtonLabels();
      AddMissingAssetLabels();
      ApplySelectableColors();
    }
  }

  static void SetToggleLabel(Toggle toggle, bool isOn)
  {
    if (toggle.targetGraphic is not Image bg)
    {
      return;
    }

    var existing = bg.transform.Find("UIDebugTheme_Label");
    GameObject labelGO;

    if (existing != null)
    {
      labelGO = existing.gameObject;
    }
    else
    {
      labelGO = new GameObject("UIDebugTheme_Label");
      labelGO.transform.SetParent(bg.transform, false);

      var rt = labelGO.AddComponent<RectTransform>();
      rt.anchorMin = Vector2.zero;
      rt.anchorMax = Vector2.one;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;

      var tmp = labelGO.AddComponent<TextMeshProUGUI>();
      tmp.alignment = TextAlignmentOptions.Center;
      tmp.color = Color.white;
      tmp.fontStyle = FontStyles.Bold;
      tmp.enableAutoSizing = true;
      tmp.fontSizeMin = 8;
      tmp.fontSizeMax = 24;

      _addedLabels.Add(labelGO);
    }

    labelGO.GetComponent<TextMeshProUGUI>().text = isOn ? "ON" : "OFF";
  }

  // For Images named "index" whose sprite is missing, add a text label derived from
  // the parent GO's name. This covers numbered icon slots like player-index badges
  // where the sprite asset provides the numeral.
  static void AddMissingAssetLabels()
  {
    var images = Object.FindObjectsOfType<Image>(true);
    foreach (var image in images)
    {
      if (image.gameObject.name != "index") { continue; }
      if (!_originals.ContainsKey(image.GetInstanceID())) { continue; }
      if (image.transform.Find("UIDebugTheme_IconLabel") != null) { continue; }

      string text = image.transform.parent != null ? image.transform.parent.name : null;
      if (string.IsNullOrEmpty(text)) { continue; }

      var labelGO = new GameObject("UIDebugTheme_IconLabel");
      labelGO.transform.SetParent(image.transform, false);

      var lrt = labelGO.AddComponent<RectTransform>();
      lrt.anchorMin = Vector2.zero;
      lrt.anchorMax = Vector2.one;
      lrt.offsetMin = Vector2.zero;
      lrt.offsetMax = Vector2.zero;

      var tmp = labelGO.AddComponent<TextMeshProUGUI>();
      tmp.text = text;
      tmp.alignment = TextAlignmentOptions.Center;
      tmp.color = Color.white;
      tmp.fontStyle = FontStyles.Bold;
      tmp.enableAutoSizing = true;
      tmp.fontSizeMin = 8;
      tmp.fontSizeMax = 36;

      _addedLabels.Add(labelGO);
    }
  }

  static void ApplySelectableColors()
  {
    var selectables = Object.FindObjectsOfType<Selectable>(true);
    foreach (var sel in selectables)
    {
      if (sel.targetGraphic is not Image img) { continue; }

      bool isInputField = sel is TMP_InputField || sel is InputField;

      // For input fields, apply theming regardless of whether ApplyTheme tracked
      // the background image (it may have been skipped by the white/sprite filter).
      // For all other selectables, only theme images that have missing sprites.
      if (!isInputField && !_originals.ContainsKey(img.GetInstanceID())) { continue; }

      int id = sel.GetInstanceID();
      if (_originalBlocks.ContainsKey(id)) { continue; }

      _originalTransitions[id] = sel.transition;
      _originalBlocks[id] = sel.colors;

      int imgId = img.GetInstanceID();
      if (!_originals.ContainsKey(imgId)) { _originals[imgId] = img.color; }

      if (isInputField)
      {
        // If the image has a non-default material (e.g. UI/ColorAdditive) it will
        // render additively and always appear washed out white regardless of image.color.
        // Override it to null (Unity's default UI/Default alpha-blend shader) so our
        // color values actually show up.
        if (img.material != img.defaultMaterial && !_originalMaterials.ContainsKey(imgId))
        {
          _originalMaterials[imgId] = img.material;
          img.material = null;
        }

        // Light gray normal state so the field is visible; blue tint on focus.
        sel.transition = Selectable.Transition.ColorTint;
        sel.colors = InputFieldBlock;
      }
      else if (sel is Toggle)
      {
        // Toggle colors are managed entirely by RegisterToggleListeners/PollToggleStates.
        // Setting ColorTint here causes CrossFadeColor(normalColor) to overwrite those
        // colors after each click, so disable the transition instead.
        sel.transition = Selectable.Transition.None;
      }
      else
      {
        sel.transition = Selectable.Transition.ColorTint;
        sel.colors = ThemedBlock;
      }
    }
  }

  static void CleanupToggleTracking()
  {
    foreach (var label in _addedLabels)
    {
      if (label != null)
      {
        Object.Destroy(label);
      }
    }
    _addedLabels.Clear();
    _lastToggleStates.Clear();
  }

}
