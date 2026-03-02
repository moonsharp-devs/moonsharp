using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MoonSharp.UnityTestBed
{
	public static class BatchRunner
	{
		private const string ScenePath = "Assets/Tests.unity";
		private const double TimeoutSeconds = 1800;

		private static DateTime s_StartTimeUtc;
		private static int? s_PendingExitCode;
		private static Type s_TestRunnerType;
		private static PropertyInfo s_IsStartedProperty;
		private static PropertyInfo s_IsCompletedProperty;
		private static PropertyInfo s_FailCountProperty;
		private static PropertyInfo s_FinalSummaryProperty;

		public static void Run()
		{
			Debug.Log("BatchRunner: opening scene " + ScenePath);
			EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

			if (!ResolveTestRunnerType())
				return;

			EnsureTestRunnerExistsInScene();

			s_StartTimeUtc = DateTime.UtcNow;
			s_PendingExitCode = null;
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
			EditorApplication.isPlaying = true;
		}

		private static void Update()
		{
			if ((DateTime.UtcNow - s_StartTimeUtc).TotalSeconds > TimeoutSeconds)
			{
				FailAndExit("BatchRunner timeout reached.");
				return;
			}

			if (s_PendingExitCode.HasValue && !EditorApplication.isPlaying)
			{
				Exit(s_PendingExitCode.Value);
				return;
			}

			if (!EditorApplication.isPlaying)
			{
				return;
			}

			if (!GetBool(s_IsStartedProperty))
			{
				return;
			}

			if (!GetBool(s_IsCompletedProperty))
			{
				return;
			}

			Debug.Log("BatchRunner: " + (GetString(s_FinalSummaryProperty) ?? "Completed."));

			if (GetInt(s_FailCountProperty) > 0)
			{
				s_PendingExitCode = 1;
				EditorApplication.isPlaying = false;
				return;
			}

			s_PendingExitCode = 0;
			EditorApplication.isPlaying = false;
		}

		private static void FailAndExit(string message)
		{
			Debug.LogError(message);
			if (EditorApplication.isPlaying)
			{
				s_PendingExitCode = 2;
				EditorApplication.isPlaying = false;
				return;
			}
			Exit(2);
		}

		private static void Exit(int code)
		{
			EditorApplication.update -= Update;
			EditorApplication.Exit(code);
		}

		private static bool ResolveTestRunnerType()
		{
			if (s_TestRunnerType != null)
				return true;

			s_TestRunnerType = Type.GetType("TestRunner, Assembly-CSharp");
			if (s_TestRunnerType == null)
			{
				FailAndExit("BatchRunner could not resolve type 'TestRunner' in Assembly-CSharp.");
				return false;
			}

			s_IsStartedProperty = s_TestRunnerType.GetProperty("IsStarted", BindingFlags.Public | BindingFlags.Static);
			s_IsCompletedProperty = s_TestRunnerType.GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Static);
			s_FailCountProperty = s_TestRunnerType.GetProperty("FailCount", BindingFlags.Public | BindingFlags.Static);
			s_FinalSummaryProperty = s_TestRunnerType.GetProperty("FinalSummary", BindingFlags.Public | BindingFlags.Static);

			return true;
		}

		private static void EnsureTestRunnerExistsInScene()
		{
			if (UnityEngine.Object.FindObjectOfType(s_TestRunnerType) != null)
				return;

			var go = new GameObject("TestRunner");
			go.AddComponent(s_TestRunnerType);
			Debug.Log("BatchRunner: created TestRunner GameObject.");
		}

		private static bool GetBool(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				return false;
			return (bool)propertyInfo.GetValue(null, null);
		}

		private static int GetInt(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				return 0;
			return (int)propertyInfo.GetValue(null, null);
		}

		private static string GetString(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				return null;
			return propertyInfo.GetValue(null, null) as string;
		}
	}
}
