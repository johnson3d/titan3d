using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for Transform.xaml
    /// </summary>
    public partial class Transform : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public object GameActors
        {
            get { return (object)GetValue(GameActorsProperty); }
            set { SetValue(GameActorsProperty, value); }
        }
        public static readonly DependencyProperty GameActorsProperty = DependencyProperty.Register("GameActors", typeof(object), typeof(Transform));

        public object PlacementComponent
        {
            get { return (object)GetValue(PlacementComponentProperty); }
            set { SetValue(PlacementComponentProperty, value); }
        }
        public static readonly DependencyProperty PlacementComponentProperty = DependencyProperty.Register("PlacementComponent", typeof(object), typeof(Transform), new PropertyMetadata(new PropertyChangedCallback(OnPlacementComponentChangedCallback)));
        static void OnPlacementComponentChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as Transform;
            var newValue = e.NewValue;
            var valueType = newValue.GetType();
            var posXMultiVal = new EditorCommon.PropertyMultiValue();
            var posYMultiVal = new EditorCommon.PropertyMultiValue();
            var posZMultiVal = new EditorCommon.PropertyMultiValue();
            var rotXMultiVal = new EditorCommon.PropertyMultiValue();
            var rotYMultiVal = new EditorCommon.PropertyMultiValue();
            var rotZMultiVal = new EditorCommon.PropertyMultiValue();
            var scaleXMultiVal = new EditorCommon.PropertyMultiValue();
            var scaleYMultiVal = new EditorCommon.PropertyMultiValue();
            var scaleZMultiVal = new EditorCommon.PropertyMultiValue();
            var inheritRotVal = new EditorCommon.PropertyMultiValue();
            var inheritScaleVal = new EditorCommon.PropertyMultiValue();
            var delta = 180 / System.Math.PI;
            if (valueType == typeof(EditorCommon.PropertyMultiValue))
            {
                var multiValue = newValue as EditorCommon.PropertyMultiValue;
                foreach (EngineNS.GamePlay.Component.GPlacementComponent placement in multiValue.Values)
                {
                    placement.PropertyChanged -= ctrl.Placement_PropertyChanged;
                    placement.PropertyChanged += ctrl.Placement_PropertyChanged;

                    posXMultiVal.Values.Add(placement.Location.X);
                    posYMultiVal.Values.Add(placement.Location.Y);
                    posZMultiVal.Values.Add(placement.Location.Z);

                    float yaw, pitch, roll;
                    placement.Rotation.GetYawPitchRoll(out yaw, out pitch, out roll);
                    rotXMultiVal.Values.Add(pitch * delta);
                    rotYMultiVal.Values.Add(yaw * delta);
                    rotZMultiVal.Values.Add(roll * delta);

                    scaleXMultiVal.Values.Add(placement.Scale.X);
                    scaleYMultiVal.Values.Add(placement.Scale.Y);
                    scaleZMultiVal.Values.Add(placement.Scale.Z);

                    inheritRotVal.Values.Add(placement.InheritRotation);
                    inheritScaleVal.Values.Add(placement.InheritScale);
                }
            }
            else// if(valueType == typeof(EngineNS.GamePlay.Component.GPlacementComponent))
            {
                var placement = newValue as EngineNS.GamePlay.Component.GPlacementComponent;
                placement.PropertyChanged -= ctrl.Placement_PropertyChanged;
                placement.PropertyChanged += ctrl.Placement_PropertyChanged;

                posXMultiVal.Values.Add(placement.Location.X);
                posYMultiVal.Values.Add(placement.Location.Y);
                posZMultiVal.Values.Add(placement.Location.Z);

                float yaw, pitch, roll;
                placement.Rotation.GetYawPitchRoll(out yaw, out pitch, out roll);
                rotXMultiVal.Values.Add(pitch * delta);
                rotYMultiVal.Values.Add(yaw * delta);
                rotZMultiVal.Values.Add(roll * delta);

                scaleXMultiVal.Values.Add(placement.Scale.X);
                scaleYMultiVal.Values.Add(placement.Scale.Y);
                scaleZMultiVal.Values.Add(placement.Scale.Z);

                inheritRotVal.Values.Add(placement.InheritRotation);
                inheritScaleVal.Values.Add(placement.InheritScale);
            }
            ctrl.mPositionX = posXMultiVal.GetValue();
            ctrl.OnPropertyChanged("PositionX");
            ctrl.mPositionY = posYMultiVal.GetValue();
            ctrl.OnPropertyChanged("PositionY");
            ctrl.mPositionZ = posZMultiVal.GetValue();
            ctrl.OnPropertyChanged("PositionZ");

            ctrl.mRotationX = rotXMultiVal.GetValue();
            ctrl.mOldRotationX = ctrl.mRotationX;
            ctrl.OnPropertyChanged("RotationX");
            ctrl.mRotationY = rotYMultiVal.GetValue();
            ctrl.mOldRotationY = ctrl.mRotationY;
            ctrl.OnPropertyChanged("RotationY");
            ctrl.mRotationZ = rotZMultiVal.GetValue();
            ctrl.mOldRotationZ = ctrl.mRotationZ;
            ctrl.OnPropertyChanged("RotationZ");

            ctrl.mScaleX = scaleXMultiVal.GetValue();
            ctrl.OnPropertyChanged("ScaleX");
            ctrl.mScaleY = scaleYMultiVal.GetValue();
            ctrl.OnPropertyChanged("ScaleY");
            ctrl.mScaleZ = scaleZMultiVal.GetValue();
            ctrl.OnPropertyChanged("ScaleZ");

            ctrl.mInheritRotation = inheritRotVal.GetValue();
            ctrl.OnPropertyChanged("InheritRotation");

            ctrl.mInheritScale = inheritScaleVal.GetValue();
            ctrl.OnPropertyChanged("InheritScale");
        }

        bool mIsManual = false;
        private void Placement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (mIsManual)
                return;
            var newValue = PlacementComponent;
            var valueType = newValue.GetType();
            var posXMultiVal = new EditorCommon.PropertyMultiValue();
            var posYMultiVal = new EditorCommon.PropertyMultiValue();
            var posZMultiVal = new EditorCommon.PropertyMultiValue();
            var rotXMultiVal = new EditorCommon.PropertyMultiValue();
            var rotYMultiVal = new EditorCommon.PropertyMultiValue();
            var rotZMultiVal = new EditorCommon.PropertyMultiValue();
            var scaleXMultiVal = new EditorCommon.PropertyMultiValue();
            var scaleYMultiVal = new EditorCommon.PropertyMultiValue();
            var scaleZMultiVal = new EditorCommon.PropertyMultiValue();
            var inheritRotVal = new EditorCommon.PropertyMultiValue();
            var inheritScaleVal = new EditorCommon.PropertyMultiValue();
            var delta = 180 / System.Math.PI;
            if (valueType == typeof(EditorCommon.PropertyMultiValue))
            {
                var multiValue = newValue as EditorCommon.PropertyMultiValue;
                foreach (EngineNS.GamePlay.Component.GPlacementComponent placement in multiValue.Values)
                {
                    posXMultiVal.Values.Add(placement.Location.X);
                    posYMultiVal.Values.Add(placement.Location.Y);
                    posZMultiVal.Values.Add(placement.Location.Z);

                    float yaw, pitch, roll;
                    placement.Rotation.GetYawPitchRoll(out yaw, out pitch, out roll);
                    rotXMultiVal.Values.Add(pitch * delta);
                    rotYMultiVal.Values.Add(yaw * delta);
                    rotZMultiVal.Values.Add(roll * delta);

                    scaleXMultiVal.Values.Add(placement.Scale.X);
                    scaleYMultiVal.Values.Add(placement.Scale.Y);
                    scaleZMultiVal.Values.Add(placement.Scale.Z);

                    inheritRotVal.Values.Add(placement.InheritRotation);
                    inheritScaleVal.Values.Add(placement.InheritScale);
                }
            }
            else// if (valueType == typeof(EngineNS.GamePlay.Component.GPlacementComponent))
            {
                var placement = newValue as EngineNS.GamePlay.Component.GPlacementComponent;

                posXMultiVal.Values.Add(placement.Location.X);
                posYMultiVal.Values.Add(placement.Location.Y);
                posZMultiVal.Values.Add(placement.Location.Z);

                float yaw, pitch, roll;
                placement.Rotation.GetYawPitchRoll(out yaw, out pitch, out roll);
                rotXMultiVal.Values.Add(pitch * delta);
                rotYMultiVal.Values.Add(yaw * delta);
                rotZMultiVal.Values.Add(roll * delta);

                scaleXMultiVal.Values.Add(placement.Scale.X);
                scaleYMultiVal.Values.Add(placement.Scale.Y);
                scaleZMultiVal.Values.Add(placement.Scale.Z);

                inheritRotVal.Values.Add(placement.InheritRotation);
                inheritScaleVal.Values.Add(placement.InheritScale);
            }
            mPositionX = posXMultiVal.GetValue();
            OnPropertyChanged("PositionX");
            mPositionY = posYMultiVal.GetValue();
            OnPropertyChanged("PositionY");
            mPositionZ = posZMultiVal.GetValue();
            OnPropertyChanged("PositionZ");

            var RotationAngle = GameActors as WPG.Themes.TypeEditors.TransformGradient.IRotationAngle;
            if (RotationAngle != null)
            {
                //RotationAngle.YawPitchRoll = new EngineNS.Vector3(System.Convert.ToSingle(mRotationX), System.Convert.ToSingle(mRotationY), System.Convert.ToSingle(mRotationZ));
                mRotationX = RotationAngle.YawPitchRoll.X;
                mOldRotationX = mRotationX;
                OnPropertyChanged("RotationX");
                mRotationY = RotationAngle.YawPitchRoll.Y;
                mOldRotationY = mRotationY;
                OnPropertyChanged("RotationY");
                mRotationZ = RotationAngle.YawPitchRoll.Z;
                mOldRotationZ = mRotationZ;
                OnPropertyChanged("RotationZ");
            }
            else
            {
                mRotationX = rotXMultiVal.GetValue();
                mOldRotationX = mRotationX;
                OnPropertyChanged("RotationX");
                mRotationY = rotYMultiVal.GetValue();
                mOldRotationY = mRotationY;
                OnPropertyChanged("RotationY");
                mRotationZ = rotZMultiVal.GetValue();
                mOldRotationZ = mRotationZ;
                OnPropertyChanged("RotationZ");
            }
          

            mScaleX = scaleXMultiVal.GetValue();
            OnPropertyChanged("ScaleX");
            mScaleY = scaleYMultiVal.GetValue();
            OnPropertyChanged("ScaleY");
            mScaleZ = scaleZMultiVal.GetValue();
            OnPropertyChanged("ScaleZ");

            mInheritRotation = inheritRotVal.GetValue();
            OnPropertyChanged("InheritRotation");

            mInheritScale = inheritScaleVal.GetValue();
            OnPropertyChanged("InheritScale");
        }

        void UpdateActorsTranslation()
        {
            try
            {
                var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    int index = 0;
                    foreach (var actorObj in (IEnumerable)GameActors)
                    {
                        var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;

                        float x, y, z;
                        if (mPositionX is EditorCommon.PropertyMultiValue)
                            x = (float)((EditorCommon.PropertyMultiValue)mPositionX).Values[index];
                        else
                            x = System.Convert.ToSingle(mPositionX);
                        if (mPositionY is EditorCommon.PropertyMultiValue)
                            y = (float)((EditorCommon.PropertyMultiValue)mPositionY).Values[index];
                        else
                            y = System.Convert.ToSingle(mPositionY);
                        if (mPositionZ is EditorCommon.PropertyMultiValue)
                            z = (float)((EditorCommon.PropertyMultiValue)mPositionZ).Values[index];
                        else
                            z = System.Convert.ToSingle(mPositionZ);

                        actor.Placement.Location = new EngineNS.Vector3(x, y, z);
                        index++;
                    }
                }
                else
                {
                    //var actor = GameActors as EngineNS.GamePlay.Actor.GActor;
                    var actor = GameActors as EngineNS.GamePlay.Component.IPlaceable;
                    actor.Placement.Location = new EngineNS.Vector3(System.Convert.ToSingle(mPositionX), System.Convert.ToSingle(mPositionY), System.Convert.ToSingle(mPositionZ));
                }
            }
            catch (System.Exception)
            {

            }
        }

        object mPositionX;
        public object PositionX
        {
            get { return mPositionX; }
            set
            {
                //if (System.Math.Abs(mPositionX - value) < 0.0001f)
                //    return;

                var oldPosition = mPositionX;
                mPositionX = value;
                UpdateActorsTranslation();
                OnPropertyChanged("PositionX");
            }
        }
        object mPositionY;
        public object PositionY
        {
            get { return mPositionY; }
            set
            {
                //if (System.Math.Abs(mPositionY - value) < 0.0001f)
                //    return;

                var oldPosition = mPositionY;
                mPositionY = value;
                UpdateActorsTranslation();
                OnPropertyChanged("PositionY");
            }
        }
        object mPositionZ;
        public object PositionZ
        {
            get { return mPositionZ; }
            set
            {
                //if (System.Math.Abs(mPositionZ - value) < 0.0001f)
                //    return;

                var oldPosition = mPositionZ;
                mPositionZ = value;
                UpdateActorsTranslation();
                OnPropertyChanged("PositionZ");
            }
        }

        async Task UpdateActorsRotation()
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                mIsManual = true;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
            var delta = (float)(System.Math.PI / 180);
            var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
            if (enumrableInterface != null)
            {
                int index = 0;
                foreach (var actorObj in (IEnumerable)GameActors)
                {
                    var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;

                    float yaw, pitch, roll;
                    if (mRotationX is EditorCommon.PropertyMultiValue)
                        pitch = (((float)(((EditorCommon.PropertyMultiValue)mRotationX).Values[index]))) * delta;
                    else
                        pitch = (System.Convert.ToSingle(mRotationX)) * delta;
                    if (mRotationY is EditorCommon.PropertyMultiValue)
                        yaw = (((float)(((EditorCommon.PropertyMultiValue)mRotationY).Values[index]))) * delta;
                    else
                        yaw = (System.Convert.ToSingle(mRotationY)) * delta;
                    if (mRotationZ is EditorCommon.PropertyMultiValue)
                        roll = (((float)(((EditorCommon.PropertyMultiValue)mRotationZ).Values[index]))) * delta;
                    else
                        roll = (System.Convert.ToSingle(mRotationZ)) * delta;

                    actor.Placement.Rotation = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
                }
            }
            else
            {
                //var actor = GameActors as EngineNS.GamePlay.Actor.GActor;
                var actor = GameActors as EngineNS.GamePlay.Component.IPlaceable;
                actor.Placement.Rotation = EngineNS.Quaternion.RotationYawPitchRoll((System.Convert.ToSingle(mRotationY)) * delta,
                                                                                    (System.Convert.ToSingle(mRotationX)) * delta,
                                                                                    (System.Convert.ToSingle(mRotationZ)) * delta);

                var RotationAngle = GameActors as WPG.Themes.TypeEditors.TransformGradient.IRotationAngle;
                if (RotationAngle != null)
                {
                    RotationAngle.YawPitchRoll = new EngineNS.Vector3(System.Convert.ToSingle(mRotationX), System.Convert.ToSingle(mRotationY), System.Convert.ToSingle(mRotationZ));
                }
            }
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                mIsManual = false;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }
        object mOldRotationX;
        object mRotationX = 0;
        public object RotationX
        {
            get { return mRotationX; }
            set
            {
                //if (System.Math.Abs(mRotationX - value) < 0.0001f)
                //    return;

                mOldRotationX = mRotationX;
                mRotationX = value;
                var noUse = UpdateActorsRotation();
                OnPropertyChanged("RotationX");
            }
        }
        object mOldRotationY;
        object mRotationY = 0;
        public object RotationY
        {
            get { return mRotationY; }
            set
            {
                //if (System.Math.Abs(mRotationY - value) < 0.0001f)
                //    return;

                mOldRotationY = mRotationY;
                mRotationY = value;
                var noUse = UpdateActorsRotation();
                OnPropertyChanged("RotationY");
            }
        }
        object mOldRotationZ;
        object mRotationZ = 0;
        public object RotationZ
        {
            get { return mRotationZ; }
            set
            {
                //if (System.Math.Abs(mRotationZ - value) < 0.0001f)
                //    return;

                mOldRotationZ = mRotationZ;
                mRotationZ = value;
                var noUse = UpdateActorsRotation();
                OnPropertyChanged("RotationZ");
            }
        }
        bool mLockXYZ = true;
        public bool LockXYZ
        {
            get { return mLockXYZ; }
            set
            {
                mLockXYZ = value;
                OnPropertyChanged("LockXYZ");
            }
        }
        void UpdateScaleX(float value)
        {
            var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
            float ratio = 1;
            if (enumrableInterface != null)
            {
                int index = 0;
                foreach (var actorObj in (IEnumerable)GameActors)
                {
                    var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;
                    if (mScaleX is EditorCommon.PropertyMultiValue)
                    {
                        if (LockXYZ)
                            ratio = value / (float)((EditorCommon.PropertyMultiValue)mScaleX).Values[index];
                        ((EditorCommon.PropertyMultiValue)mScaleX).Values[index] = value;
                    }
                    else
                    {
                        if (LockXYZ)
                            ratio = value / System.Convert.ToSingle(mScaleX);
                        mScaleX = value;
                    }
                    if (mScaleY is EditorCommon.PropertyMultiValue)
                        ((EditorCommon.PropertyMultiValue)mScaleY).Values[index] = (float)((EditorCommon.PropertyMultiValue)mScaleY).Values[index] * ratio;
                    else
                        mScaleY = System.Convert.ToSingle(mScaleY) * ratio;
                    if (mScaleZ is EditorCommon.PropertyMultiValue)
                        ((EditorCommon.PropertyMultiValue)mScaleZ).Values[index] = (float)((EditorCommon.PropertyMultiValue)mScaleZ).Values[index] * ratio;
                    else
                        mScaleZ = System.Convert.ToSingle(mScaleZ) * ratio;
                    index++;
                }
            }
            else
            {
                if (LockXYZ)
                    ratio = value / (float)mScaleX;
                mScaleX = value;
                mScaleY = ratio * (float)mScaleY;
                mScaleZ = ratio * (float)mScaleZ;
            }
        }
        void UpdateScaleY(float value)
        {
            var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
            float ratio = 1;
            if (enumrableInterface != null)
            {
                int index = 0;
                foreach (var actorObj in (IEnumerable)GameActors)
                {
                    var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;
                    if (mScaleY is EditorCommon.PropertyMultiValue)
                    {
                        if (LockXYZ)
                            ratio = value / (float)((EditorCommon.PropertyMultiValue)mScaleY).Values[index];
                        ((EditorCommon.PropertyMultiValue)mScaleY).Values[index] = value;
                    }
                    else
                    {
                        if (LockXYZ)
                            ratio = value / System.Convert.ToSingle(mScaleY);
                        mScaleY = value;
                    }
                    if (mScaleX is EditorCommon.PropertyMultiValue)
                        ((EditorCommon.PropertyMultiValue)mScaleX).Values[index] = (float)((EditorCommon.PropertyMultiValue)mScaleX).Values[index] * ratio;
                    else
                        mScaleX = System.Convert.ToSingle(mScaleX) * ratio;
                    if (mScaleZ is EditorCommon.PropertyMultiValue)
                        ((EditorCommon.PropertyMultiValue)mScaleZ).Values[index] = (float)((EditorCommon.PropertyMultiValue)mScaleZ).Values[index] * ratio;
                    else
                        mScaleZ = System.Convert.ToSingle(mScaleZ) * ratio;
                    index++;
                }
            }
            else
            {
                if (LockXYZ)
                    ratio = value / (float)mScaleY;
                mScaleY = value;
                mScaleX = ratio * (float)mScaleX;
                mScaleZ = ratio * (float)mScaleZ;
            }
        }
        void UpdateScaleZ(float value)
        {
            var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
            float ratio = 1;
            if (enumrableInterface != null)
            {
                int index = 0;
                foreach (var actorObj in (IEnumerable)GameActors)
                {
                    var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;
                    if (mScaleZ is EditorCommon.PropertyMultiValue)
                    {
                        if (LockXYZ)
                            ratio = value / (float)((EditorCommon.PropertyMultiValue)mScaleZ).Values[index];
                        ((EditorCommon.PropertyMultiValue)mScaleZ).Values[index] = value;
                    }
                    else
                    {
                        if (LockXYZ)
                            ratio = value / System.Convert.ToSingle(mScaleZ);
                        mScaleZ = value;
                    }
                    if (mScaleY is EditorCommon.PropertyMultiValue)
                        ((EditorCommon.PropertyMultiValue)mScaleY).Values[index] = (float)((EditorCommon.PropertyMultiValue)mScaleY).Values[index] * ratio;
                    else
                        mScaleY = System.Convert.ToSingle(mScaleY) * ratio;
                    if (mScaleX is EditorCommon.PropertyMultiValue)
                        ((EditorCommon.PropertyMultiValue)mScaleX).Values[index] = (float)((EditorCommon.PropertyMultiValue)mScaleX).Values[index] * ratio;
                    else
                        mScaleX = System.Convert.ToSingle(mScaleX) * ratio;
                    index++;
                }
            }
            else
            {
                if (LockXYZ)
                    ratio = value / (float)mScaleZ;
                mScaleZ = value;
                mScaleY = ratio * (float)mScaleY;
                mScaleX = ratio * (float)mScaleX;
            }
        }
        void UpdateActorsScale()
        {
            var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
            if (enumrableInterface != null)
            {
                int index = 0;
                foreach (var actorObj in (IEnumerable)GameActors)
                {
                    var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;
                    float x, y, z;
                    if (mScaleX is EditorCommon.PropertyMultiValue)
                        x = (float)((EditorCommon.PropertyMultiValue)mScaleX).Values[index];
                    else
                        x = System.Convert.ToSingle(mScaleX);
                    if (mScaleY is EditorCommon.PropertyMultiValue)
                        y = (float)((EditorCommon.PropertyMultiValue)mScaleY).Values[index];
                    else
                        y = System.Convert.ToSingle(mScaleY);
                    if (mScaleZ is EditorCommon.PropertyMultiValue)
                        z = (float)((EditorCommon.PropertyMultiValue)mScaleZ).Values[index];
                    else
                        z = System.Convert.ToSingle(mScaleZ);
                    actor.Placement.Scale = new EngineNS.Vector3(x, y, z);
                    index++;
                }
            }
            else
            {
                //var actor = GameActors as EngineNS.GamePlay.Actor.GActor;
                var actor = GameActors as EngineNS.GamePlay.Component.IPlaceable;
                actor.Placement.Scale = new EngineNS.Vector3(System.Convert.ToSingle(mScaleX), System.Convert.ToSingle(mScaleY), System.Convert.ToSingle(mScaleZ));
            }
        }

        object mInheritRotation = true;
        public object InheritRotation
        {
            get { return mInheritRotation; }
            set
            {
                mInheritRotation = value;
                UpdateActorsInheritRotation();
                OnPropertyChanged("InheritRotation");
            }
        }
        object mInheritScale = true;
        public object InheritScale
        {
            get { return mInheritScale; }
            set
            {
                mInheritScale = value;
                UpdateActorsInheritScale();
                OnPropertyChanged("InheritScale");
            }
        }
        void UpdateActorsInheritRotation()
        {
            var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
            if (enumrableInterface != null)
            {
                int index = 0;
                foreach (var actorObj in (IEnumerable)GameActors)
                {
                    var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;
                    bool inheritRotation;
                    if (mInheritRotation is EditorCommon.PropertyMultiValue)
                        inheritRotation = (bool)((EditorCommon.PropertyMultiValue)mInheritRotation).Values[index];
                    else
                        inheritRotation = System.Convert.ToBoolean(mInheritRotation);
                    actor.Placement.InheritRotation = inheritRotation;
                    index++;
                }
            }
            else
            {
                //var actor = GameActors as EngineNS.GamePlay.Actor.GActor;
                var actor = GameActors as EngineNS.GamePlay.Component.IPlaceable;
                actor.Placement.InheritRotation = (bool)mInheritRotation;
            }
        }
        void UpdateActorsInheritScale()
        {
            var enumrableInterface = GameActors.GetType().GetInterface(typeof(IEnumerable).FullName, false);
            if (enumrableInterface != null)
            {
                int index = 0;
                foreach (var actorObj in (IEnumerable)GameActors)
                {
                    var actor = actorObj as EngineNS.GamePlay.Component.IPlaceable;
                    bool inheritScale;
                    if (mInheritScale is EditorCommon.PropertyMultiValue)
                        inheritScale = (bool)((EditorCommon.PropertyMultiValue)mInheritScale).Values[index];
                    else
                        inheritScale = System.Convert.ToBoolean(mInheritScale);
                    actor.Placement.InheritScale = inheritScale;
                    index++;
                }
            }
            else
            {
                //var actor = GameActors as EngineNS.GamePlay.Actor.GActor;
                var actor = GameActors as EngineNS.GamePlay.Component.IPlaceable;
                actor.Placement.InheritScale = (bool)mInheritScale;
            }
        }
        protected bool IsNumberic(object numStr, out float result)
        {
            if (numStr is string)
            {
                if (float.TryParse((string)numStr, out result))
                {
                    return true;
                }
            }
            if (numStr is Single)
            {
                result = (float)numStr;
                return true;
            }
            result = 0;
            return false;
        }
        object mScaleX = 1;
        public object ScaleX
        {
            get { return mScaleX; }
            set
            {
                //if (System.Math.Abs(mScaleX - value) < 0.0001f)
                //    return;
                float floatValue = 1;
                if (!IsNumberic(value, out floatValue))
                    return;
                if (floatValue == 0)
                    return;
                UpdateScaleX(floatValue);
                UpdateActorsScale();
                OnPropertyChanged("ScaleX");
            }
        }
        object mScaleY = 1;
        public object ScaleY
        {
            get { return mScaleY; }
            set
            {
                float floatValue = 1;
                if (!IsNumberic(value, out floatValue))
                    return;
                if (floatValue == 0)
                    return;
                UpdateScaleY(floatValue);
                UpdateActorsScale();
                OnPropertyChanged("ScaleY");
            }
        }
        object mScaleZ = 1;
        public object ScaleZ
        {
            get { return mScaleZ; }
            set
            {
                //if (System.Math.Abs(mScaleZ - value) < 0.0001f)
                //    return;
                float floatValue = 1;
                if (!IsNumberic(value, out floatValue))
                    return;
                if (floatValue == 0)
                    return;
                UpdateScaleZ(floatValue);
                UpdateActorsScale();
                OnPropertyChanged("ScaleZ");
            }
        }

        public Transform()
        {
            InitializeComponent();
        }
    }
}
