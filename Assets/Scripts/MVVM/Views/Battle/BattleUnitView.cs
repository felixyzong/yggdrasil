using System.ComponentModel;
using MVVM.ViewModels.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.Views.Battle
{
    public class BattleUnitView : MonoBehaviour
    {
        [Header("Prefab Parts")]
        [SerializeField] private Transform characterRoot;
        [SerializeField] private Transform shadeRoot;
        [SerializeField] private SpriteRenderer characterSpriteRenderer;
        [SerializeField] private Image characterImage;
        [SerializeField] private SpriteRenderer shadeSpriteRenderer;
        [SerializeField] private Image shadeImage;

        [Header("Legacy Fallback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Image image;

        [Header("Interaction")]
        [SerializeField] private Button button;

        [Header("Optional UI")]
        [SerializeField] private Text nameText;
        [SerializeField] private Image healthFill;
        [SerializeField] private GameObject currentActorIndicator;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject targetIndicator;

        [Header("Colors")]
        [SerializeField] private Color aliveColor = Color.white;
        [SerializeField] private Color deadColor = new Color(0.35f, 0.35f, 0.35f, 0.7f);
        [SerializeField] private Color shadeAliveColor = new Color(0f, 0f, 0f, 0.35f);
        [SerializeField] private Color shadeDeadColor = new Color(0f, 0f, 0f, 0f);

        [Header("Auto Resolve")]
        [SerializeField] private bool autoResolvePrefabParts = true;
        [SerializeField] private string characterChildName = "Character";
        [SerializeField] private string shadeChildName = "Shade";

        private BattleGridView gridView;
        private CharacterSpriteCatalog spriteCatalog;

        public BattleUnitViewModel ViewModel { get; private set; }

        private void Awake()
        {
            ResolvePrefabParts();
        }

        public void Bind(BattleUnitViewModel viewModel, BattleGridView owner, CharacterSpriteCatalog catalog)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelChanged;
            }

            ViewModel = viewModel;
            gridView = owner;
            spriteCatalog = catalog;
            ResolvePrefabParts();

            if (button != null)
            {
                button.onClick.RemoveListener(Select);
                button.onClick.AddListener(Select);
            }

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelChanged;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelChanged;
            }

            if (button != null)
            {
                button.onClick.RemoveListener(Select);
            }
        }

        private void OnMouseDown()
        {
            Select();
        }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        public void Select()
        {
            if (ViewModel == null) return;
            gridView?.SelectUnit(ViewModel);
        }

        private void Refresh()
        {
            if (ViewModel == null) return;

            Sprite sprite = spriteCatalog != null ? spriteCatalog.GetSprite(ViewModel.SpriteKey) : null;
            Color characterColor = ViewModel.IsDead ? deadColor : aliveColor;

            ApplyCharacterSprite(sprite, characterColor);
            ApplyShade(ViewModel.IsShadeVisible);

            if (nameText != null)
            {
                nameText.text = ViewModel.Name;
            }

            if (healthFill != null)
            {
                healthFill.fillAmount = ViewModel.HealthRatio;
            }

            if (currentActorIndicator != null)
            {
                currentActorIndicator.SetActive(ViewModel.IsCurrentActor);
            }

            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(ViewModel.IsSelected);
            }

            if (targetIndicator != null)
            {
                targetIndicator.SetActive(ViewModel.CanBeTargeted);
            }
        }

        private void ApplyCharacterSprite(Sprite sprite, Color color)
        {
            if (characterSpriteRenderer != null)
            {
                characterSpriteRenderer.sprite = sprite;
                characterSpriteRenderer.color = color;
            }

            if (characterImage != null)
            {
                characterImage.sprite = sprite;
                characterImage.color = color;
            }

            if (spriteRenderer != null && spriteRenderer != characterSpriteRenderer)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = color;
            }

            if (image != null && image != characterImage)
            {
                image.sprite = sprite;
                image.color = color;
            }
        }

        private void ApplyShade(bool isVisible)
        {
            if (shadeRoot != null)
            {
                shadeRoot.gameObject.SetActive(isVisible);
            }

            Color shadeColor = isVisible ? shadeAliveColor : shadeDeadColor;
            if (shadeSpriteRenderer != null)
            {
                shadeSpriteRenderer.color = shadeColor;
            }

            if (shadeImage != null)
            {
                shadeImage.color = shadeColor;
            }
        }

        private void ResolvePrefabParts()
        {
            if (!autoResolvePrefabParts) return;

            if (characterRoot == null && !string.IsNullOrEmpty(characterChildName))
            {
                characterRoot = transform.Find(characterChildName);
            }

            if (shadeRoot == null && !string.IsNullOrEmpty(shadeChildName))
            {
                shadeRoot = transform.Find(shadeChildName);
            }

            if (characterSpriteRenderer == null && characterRoot != null)
            {
                characterSpriteRenderer = characterRoot.GetComponentInChildren<SpriteRenderer>(true);
            }

            if (characterImage == null && characterRoot != null)
            {
                characterImage = characterRoot.GetComponentInChildren<Image>(true);
            }

            if (shadeSpriteRenderer == null && shadeRoot != null)
            {
                shadeSpriteRenderer = shadeRoot.GetComponentInChildren<SpriteRenderer>(true);
            }

            if (shadeImage == null && shadeRoot != null)
            {
                shadeImage = shadeRoot.GetComponentInChildren<Image>(true);
            }
        }
    }
}
