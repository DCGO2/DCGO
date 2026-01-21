using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AttachShuffledNumberIDs : MonoBehaviour
{
	[MenuItem("Window/Attach/AttachShuffledNumberIDs")]
	static void Attach_ShuffledNumberIDs()
	{
		UnityEngine.Random.InitState(1145141919);

		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj.GetComponent<ShuffleDeckCode>() != null)
			{
				ShuffleDeckCode shuffleDeckCode = obj.GetComponent<ShuffleDeckCode>();

				shuffleDeckCode.ShuffledNumberIDs = new int[ConvertBinaryNumber.numbers.Length];

				for(int i=0;i< shuffleDeckCode.ShuffledNumberIDs.Length;i++)
				{
					shuffleDeckCode.ShuffledNumberIDs[i] = i;
				}

				Shuffle(shuffleDeckCode.ShuffledNumberIDs);

				EditorGUI.BeginChangeCheck();

				if (EditorGUI.EndChangeCheck())
				{
					var scene = SceneManager.GetActiveScene();
					EditorSceneManager.MarkSceneDirty(scene);
				}

				return;
			}
		}
	}

	static void Shuffle(int[] numbers)
	{
		for (int i = 0; i < numbers.Length; i++)
		{
			int temp = numbers[i]; // Œ»Ý‚Ì—v‘f‚ð—a‚¯‚Ä‚¨‚­
			int randomIndex = UnityEngine.Random.Range(0, numbers.Length); // “ü‚ê‘Ö‚¦‚éæ‚ðƒ‰ƒ“ƒ_ƒ€‚É‘I‚Ô
			numbers[i] = numbers[randomIndex]; // Œ»Ý‚Ì—v‘f‚Éã‘‚«
			numbers[randomIndex] = temp; // “ü‚ê‘Ö‚¦Œ³‚É—a‚¯‚Ä‚¨‚¢‚½—v‘f‚ð—^‚¦‚é
		}
	}
}
