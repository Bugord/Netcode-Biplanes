using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class NavigationSystem : MonoBehaviour
    {
        public static NavigationSystem Instance;

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private GameObject eventSystem;
        
        [SerializeField]
        private List<BaseScreen> screenPrefabs;

        private Dictionary<Type, BaseScreen> cachedScreenPrefabsDictionary;
        private LinkedList<BaseScreen> screensStack;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(eventSystem);
            
            Instance = this;

            cachedScreenPrefabsDictionary = new Dictionary<Type, BaseScreen>();
            screensStack = new LinkedList<BaseScreen>();

            foreach (var screenPrefab in screenPrefabs) {
                var screenType = screenPrefab.GetType();
                if (cachedScreenPrefabsDictionary.ContainsKey(screenType)) {
                    Debug.LogWarning($"[{nameof(NavigationSystem)}] Screen with type {screenType} was already added");
                    continue;
                }
                cachedScreenPrefabsDictionary.Add(screenType, screenPrefab);
            }
        }

        public void PopLast()
        {
            var lastScreen = screensStack.Last();

            lastScreen.Destroy();
            screensStack.Remove(lastScreen);

            if (screensStack.Count != 0) {
                screensStack.Last().SetActive();
            }
        }

        public void PopToRoot()
        {
            while (screensStack.Count > 1) {
                PopLast();
            }
        }

        public T Push<T>() where T : BaseScreen
        {
            var screenType = typeof(T);

            if (!cachedScreenPrefabsDictionary.ContainsKey(screenType)) {
                Debug.LogError($"Screen with type {screenType} is not registered in {nameof(NavigationSystem)}");
                return null;
            }

            if (screensStack.Count != 0) {
                screensStack.Last().SetInactive();
            }

            var screenPrefab = cachedScreenPrefabsDictionary[screenType];
            var screen = Instantiate(screenPrefab, canvas.transform);
            screensStack.AddLast(screen);
            return (T)screen;
        }

        public T Replace<T>() where T : BaseScreen
        {
            PopLast();
            return Push<T>();
        }

        public void PopScreen(BaseScreen screen)
        {
            var isLast = screenPrefabs.Last() == screen;

            screensStack.Remove(screen);
            screen.Destroy();

            if (isLast && screensStack.Count != 0) {
                screensStack.Last().SetActive();
            }
        }
    }
}