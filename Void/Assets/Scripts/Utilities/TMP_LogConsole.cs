using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Utilities
{
    public class TMP_LogConsole : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI logText;

        [Header("Settings")]
        [SerializeField] private float entryLifetime = 5f; // seconds

        private readonly Queue<string> logQueue = new Queue<string>();

        /// <summary>
        /// Add a new line to the log
        /// </summary>
        public void AddLog(string message)
        {
            logQueue.Enqueue(message);
            UpdateText();

            // Start timer to remove this entry later
            Coroutine removalRoutine = StartCoroutine(RemoveAfterTime());
        }

        private IEnumerator RemoveAfterTime()
        {
            yield return new WaitForSeconds(entryLifetime);

            if (logQueue.Count > 0)
            {
                logQueue.Dequeue();
                UpdateText();
            }
        }

        /// <summary>
        /// Updates the TextMeshPro display
        /// </summary>
        private void UpdateText()
        {
            logText.text = string.Join("\n", logQueue);
        }
    }
}