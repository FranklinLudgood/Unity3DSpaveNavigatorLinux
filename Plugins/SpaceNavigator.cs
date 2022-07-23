using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpaceNavigatorDriver {

	public abstract class SpaceNavigator : IDisposable {
		// Public runtime API
		public static Vector3 Translation {
			get { return Instance.GetTranslation(); }
		}
		public static Quaternion Rotation {
			get { return Instance.GetRotation(); }
		}
		public static Quaternion RotationInLocalCoordSys(Transform coordSys) {
			return coordSys.rotation * Rotation * Quaternion.Inverse(coordSys.rotation);
		}
		public static void SetTranslationSensitivity(float newPlayTransSens) {
			Settings.PlayTransSens = newPlayTransSens;
		}
		public static void SetRotationSensitivity(float newPlayRotSens) {
			Settings.PlayRotSens = newPlayRotSens;
		}

		protected float SubtractDeadzone(int value, float deadzone) {
			return value < 0
				? Math.Min(0, value + Math.Abs(deadzone))
				: Math.Max(0, value - Math.Abs(deadzone));
		}

		// Abstract members
		public abstract Vector3 GetTranslation();
		public abstract Quaternion GetRotation();

		#region - Singleton -
		public static SpaceNavigator Instance {
			get {
				if (_instance == null) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				_instance = SpaceNavigatorWindows.SubInstance;
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
				_instance = SpaceNavigatorMac.SubInstance;
#endif
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				_instance = SpaceNavigatorLinux.SubInstance;
#endif
				}
				return _instance;
			}
			set { _instance = value; }
		}
		private static SpaceNavigator _instance;
		#endregion - Singleton -

		#region - IDisposable -
		public abstract void Dispose();
		#endregion - IDisposable -
	}
}