﻿using UnityEngine;
using System.Text;
using System.IO;
using System;

namespace MSCMP {
#if !PUBLIC_RELEASE
	/// <summary>
	/// Development tools.
	/// </summary>
	class DevTools {

		bool devView = false;
		GameObject spawnedGo = null;

		public void OnGUI(GameObject localPlayer) {
			if (!devView) {
				return;
			}

			foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>()) {

				if (localPlayer) {
					if ((go.transform.position - localPlayer.transform.position).sqrMagnitude > 10) {
						continue;
					}
				}

				Vector3 pos = Camera.main.WorldToScreenPoint(go.transform.position);
				if (pos.z < 0.0f) {
					continue;
				}


				GUI.Label(new Rect(pos.x, Screen.height - pos.y, 500, 20), go.name);
			}
		}

		public void Update() {
			if (Input.GetKeyDown(KeyCode.F3)) {
				devView = !devView;
			}

			if (Input.GetKeyDown(KeyCode.F5)) {
				DumpWorld(Application.loadedLevelName);
			}

			if (!devView && ((ourDebugPlayer != null) && characterAnimationComponent != null)) {
				if (Input.GetKeyDown(KeyCode.Keypad7)) {
					characterAnimationComponent.CrossFade("Idle");
				}

				if (Input.GetKeyDown(KeyCode.Keypad8)) {
					characterAnimationComponent.CrossFade("Walk");
				}//test

				if (Input.GetKeyDown(KeyCode.Keypad5)) {
					characterAnimationComponent.CrossFade("Jump");
				}

				if (Input.GetKeyDown(KeyCode.Keypad4)) {
					characterAnimationComponent.CrossFade("DrunkIdle");
				}

				if (Input.GetKeyDown(KeyCode.Keypad1)) {
					if (useLean) PlayActionAnim("Lean", false);
					else PlayActionAnim("Lean", true);

					useLean = !useLean;
				}

				if (Input.GetKeyDown(KeyCode.Keypad3)) {
					if (useHitchhike){
						useHitchhike = false;
						PlayActionAnim("Hitchhike", false);
					}

					if (useFinger) PlayActionAnim("Finger", false);
					else PlayActionAnim("Finger", true);

					useFinger = !useFinger;
				}

				if (Input.GetKeyDown(KeyCode.Keypad2)) {
					if (useFinger) {
						useFinger = false;
						PlayActionAnim("Finger", false);
					}

					if (useHitchhike) PlayActionAnim("Hitchhike", false);
					else PlayActionAnim("Hitchhike", true);

					useHitchhike = !useHitchhike;
				}
			}
		}

		bool useLean = false;
		bool useFinger = false;
		bool useHitchhike = false;
		public void PlayActionAnim(string animName, bool play) {
			if (play) {
				characterAnimationComponent[animName].wrapMode = WrapMode.ClampForever;
				characterAnimationComponent[animName].speed = 1;
				characterAnimationComponent[animName].enabled = true;
				characterAnimationComponent[animName].weight = 1.0f;
			}
			else {
				characterAnimationComponent[animName].wrapMode = WrapMode.Once;
				characterAnimationComponent[animName].time = characterAnimationComponent[animName].length;
				characterAnimationComponent[animName].speed = -1;
				characterAnimationComponent[animName].weight = 1.0f;
			}
		}

		GameObject ourDebugPlayer = null;
		Animation characterAnimationComponent = null;
		public GameObject LoadCustomCharacter(GameObject localPlayer) {
			GameObject loadedModel = Client.LoadAsset<GameObject>("Assets/MPPlayerModel/MPPlayerModel.fbx");
			GameObject ourCustomPlayer = (GameObject)GameObject.Instantiate((GameObject)loadedModel);

			characterAnimationComponent = ourCustomPlayer.GetComponent<Animation>();
			characterAnimationComponent["Jump"].layer = 1;
			characterAnimationComponent["Lean"].layer = 2;
			characterAnimationComponent["Lean"].blendMode = AnimationBlendMode.Additive;
			characterAnimationComponent["Finger"].layer = 2;
			characterAnimationComponent["Finger"].blendMode = AnimationBlendMode.Additive;
			characterAnimationComponent["Hitchhike"].layer = 2;
			characterAnimationComponent["Hitchhike"].blendMode = AnimationBlendMode.Additive;

			ourCustomPlayer.transform.position = localPlayer.transform.position + Vector3.up * 0.60f + localPlayer.transform.rotation * Vector3.forward * 1.0f;
			ourCustomPlayer.transform.rotation = localPlayer.transform.rotation * Quaternion.Euler(0, 180, 0);

			return ourCustomPlayer;
		}

		public bool ApplyCustomTextures(GameObject gameObject) {
			Renderer objectRenderer = ourDebugPlayer.GetComponentInChildren<Renderer>();
			if (objectRenderer == null) return false;
			//Logger.Log("Total Materials: " + testRenderer.materials.Length);

			StreamReader SettingsFile = new StreamReader(Client.GetPath("myCharacter.ini"));
			string line = null;
			for(int i=0; i<=2;i++) {
				if ((line = SettingsFile.ReadLine()) == null) break;
				string[] LineData = line.Split('|');

				float red = Convert.ToSingle(LineData[0]);
				float green = Convert.ToSingle(LineData[1]);
				float blue = Convert.ToSingle(LineData[2]);

				Color OurColor = new Color(red, green, blue);
				objectRenderer.materials[i].color = OurColor;
			}

			SettingsFile.Close();
			return true;
		}

		public void UpdatePlayer(GameObject localPlayer) {

			if (!devView) {
				return;
			}

			// Testing New Model
			if (Input.GetKey(KeyCode.KeypadMultiply) && localPlayer) {
				ourDebugPlayer = LoadCustomCharacter(localPlayer);
				//ApplyCustomTextures(ourDebugPlayer);
			}

			// Pseudo AirBrk
			if (Input.GetKey(KeyCode.KeypadPlus) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position + Vector3.up * 5.0f;
			}
			if (Input.GetKey(KeyCode.KeypadMinus) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position - Vector3.up * 5.0f;
			}
			if (Input.GetKey(KeyCode.Keypad8) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position + localPlayer.transform.rotation * Vector3.forward * 5.0f;
			}
			if (Input.GetKey(KeyCode.Keypad2) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position - localPlayer.transform.rotation * Vector3.forward * 5.0f;
			}
			if (Input.GetKey(KeyCode.Keypad4) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position - localPlayer.transform.rotation * Vector3.right * 5.0f;
			}
			if (Input.GetKey(KeyCode.Keypad6) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position + localPlayer.transform.rotation * Vector3.right * 5.0f;
			}

			if (Input.GetKeyDown(KeyCode.G) && localPlayer) {
				PlayMakerFSM fsm = Utils.GetPlaymakerScriptByName(localPlayer, "PlayerFunctions");
				if (fsm != null) {
					fsm.SendEvent("MIDDLEFINGER");
				}
				else {
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
					go.transform.position = localPlayer.transform.position + localPlayer.transform.rotation * Vector3.forward * 2.0f;
				}
			}

			if (Input.GetKeyDown(KeyCode.I) && localPlayer) {

				StringBuilder builder = new StringBuilder();
				Utils.PrintTransformTree(localPlayer.transform, 0, (int level, string text) => {

					for (int i = 0; i < level; ++i) builder.Append("    ");
					builder.Append(text + "\n");
				});
				System.IO.File.WriteAllText(Client.GetPath("localPlayer.txt"), builder.ToString());
			}


			if (Input.GetKeyDown(KeyCode.F6) && localPlayer) {


				GameObject prefab = GameObject.Find("JONNEZ ES(Clone)");
				spawnedGo = GameObject.Instantiate(prefab);

				// Remove component that overrides spawn position of JONNEZ.
				PlayMakerFSM fsm = Utils.GetPlaymakerScriptByName(spawnedGo, "LOD");
				GameObject.Destroy(fsm);

				Vector3 direction = localPlayer.transform.rotation * Vector3.forward * 2.0f;
				spawnedGo.transform.position = localPlayer.transform.position + direction;


				/*StringBuilder builder = new StringBuilder();
				PrintTrans(go.transform, 0, (int level, string text) => {

					for (int i = 0; i < level; ++i)	builder.Append("    ");
					builder.Append(text + "\n");
				});
				System.IO.File.WriteAllText("J:\\projects\\MSCMP\\MSCMP\\Debug\\charTree.txt", builder.ToString());*/


			}
		}

		public static void DumpWorld(string levelName) {
			GameObject []gos = gos = GameObject.FindObjectsOfType<GameObject>();

			string path = Client.GetPath($"WorldDump\\{levelName}");
			Directory.CreateDirectory(path);

			StringBuilder builder = new StringBuilder();
			int index = 0;
			foreach (GameObject go in gos) {
				Transform trans = go.GetComponent<Transform>();
				if (trans == null || trans.parent != null) continue;

				string SanitizedName = go.name;
				SanitizedName = SanitizedName.Replace("/", "");
				string dumpFilePath = path + "\\" + SanitizedName + ".txt";
				try {
					DumpObject(trans, dumpFilePath);
				} catch (Exception e) {
					Logger.Log("Unable to dump objects: " + SanitizedName + "\n");
					Logger.Log(e.Message + "\n");
				}

				builder.Append(go.name + " (" + SanitizedName + "), Trans: " + trans.position.ToString() + "\n");
				++index;
			}

			System.IO.File.WriteAllText(path + "\\dumpLog.txt", builder.ToString());
		}

		public static void DumpObject(Transform obj, string file) {
			StringBuilder bldr = new StringBuilder();
			Utils.PrintTransformTree(obj, 0, (int level, string text) => {
				for (int i = 0; i < level; ++i) bldr.Append("    ");
				bldr.Append(text + "\n");
			});

			System.IO.File.WriteAllText(file, bldr.ToString());
		}
	}
#endif
}
