using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX 
namespace SpaceNavigatorDriver {
    public class SpaceNavigatorLinux : SpaceNavigator {

        private const float TransSensScale = 0.0007f, RotSensScale = 0.025f;

		private const int RotationDeadzone = 30;

		private Vector3 _position;
		private Quaternion _rotation;
		private Vector3 _eulerAngles;
		private IntPtr _xValPos;
		private IntPtr _yValPos;
		private IntPtr _zValPos;
		private IntPtr _xValRot;
		private IntPtr _yValRot;
		private IntPtr _zValRot;


        [DllImport("libspnav.so")]
        private static extern int spnav_open();

        [DllImport("libspnav.so")]
        private static extern int spnav_close();

        [DllImport("libspnav.so")]
        private static extern int spnav_remove_events(int type);

		[DllImport("libspnav.so")]
        private static extern int sample_motion(IntPtr x, IntPtr y, IntPtr z, IntPtr rx, IntPtr ry, IntPtr rz);


		private void UpDateSpaceMouse() {

			float translationSensitivity = Application.isPlaying ? Settings.PlayTransSens : Settings.TransSens[Settings.CurrentGear];
			float rotationSensitivity = Application.isPlaying ? Settings.PlayRotSens : Settings.RotSens;
			if(sample_motion( _xValPos, _yValPos, _zValPos, _xValRot, _yValRot, _zValRot) == 1)
			{
				int x = Marshal.ReadInt32(_xValPos); 
			 	int y = Marshal.ReadInt32(_yValPos);  
			 	int z = Marshal.ReadInt32(_zValPos);
				int rx = Marshal.ReadInt32(_xValRot); 
			 	int ry = Marshal.ReadInt32(_yValRot);  
			 	int rz = Marshal.ReadInt32(_zValRot);

			 	spnav_remove_events(0);
			 	_position.Set((float) x, (float) y, (float) z);
			 	_position = _position * translationSensitivity * TransSensScale;

				_eulerAngles.Set((float) rx, (float) ry, (float) rz);
				_eulerAngles = _eulerAngles * rotationSensitivity * RotSensScale;
				_rotation.eulerAngles = _eulerAngles;
			}
		}  

        // Abstract members
		public override Vector3 GetTranslation() {
			UpDateSpaceMouse();
			return _position;
		}

		public override Quaternion GetRotation() {
			UpDateSpaceMouse();
			return _rotation;
		}

        #region - Singleton -
		/// <summary>
		/// Private constructor, prevents a default instance of the <see cref="SpaceNavigatorLinux" /> class from being created.
		/// </summary>
        private SpaceNavigatorLinux() {

			if (!SessionState.GetBool("FirstTimeInit", false)) {
			
				_position = new Vector3(0.0f, 0.0f, 0.0f);
				_rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
				_eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
				_xValPos = Marshal.AllocHGlobal(sizeof(int));
				_yValPos = Marshal.AllocHGlobal(sizeof(int));
				_zValPos = Marshal.AllocHGlobal(sizeof(int));
				_xValRot = Marshal.AllocHGlobal(sizeof(int));
				_yValRot = Marshal.AllocHGlobal(sizeof(int));
				_zValRot = Marshal.AllocHGlobal(sizeof(int));

				Marshal.WriteInt32(_xValPos, 0);
				Marshal.WriteInt32(_yValPos, 0);
				Marshal.WriteInt32(_zValPos, 0);
				Marshal.WriteInt32(_xValRot, 0);
				Marshal.WriteInt32(_yValRot, 0);
				Marshal.WriteInt32(_zValRot, 0);
				
				try {
					if(spnav_open() == -1) 
					{
						Debug.LogError("Failed to connect to spacenavd library.");
					}
				}
				catch (Exception ex) {
					Debug.LogError(ex.ToString());
				}

				SessionState.SetBool("FirstTimeInit", true);
			}
        }

        private static SpaceNavigatorLinux _subInstance;

        public static SpaceNavigatorLinux SubInstance {
			get { return _subInstance ?? (_subInstance = new SpaceNavigatorLinux()); }
		}

        #endregion - Singleton -

		
        #region - IDisposable -
		public override void Dispose() {

			if(Application.isEditor) {
				try {

					if(_xValPos != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(_xValPos);
						_xValPos = IntPtr.Zero;
					}
					
					if(_yValPos != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(_yValPos);
						_yValPos = IntPtr.Zero;
					}

					if(_zValPos != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(_zValPos);
						_zValPos = IntPtr.Zero;
					}

					if(_xValRot != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(_xValRot);
						_xValRot = IntPtr.Zero;
					}

					if(_yValRot != IntPtr.Zero) 
					{
						Marshal.FreeHGlobal(_yValRot);
						_yValRot = IntPtr.Zero;
					}

					if(_zValRot != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(_zValRot);
						_zValRot = IntPtr.Zero;
					}

					if(SessionState.GetBool("FirstTimeInit", true))
					{
						if(spnav_close() == -1) 
						{
							Debug.LogError("Failed to disconnect from the spacenavd library.");
						}
					}
					
				}
				catch (Exception ex) {
					Debug.LogError(ex.ToString());
				}
			}
		}
        #endregion - IDisposable -

    }
}
#endif // UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX 