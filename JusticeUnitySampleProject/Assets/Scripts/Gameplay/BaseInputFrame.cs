// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;

namespace Game
{
    /// <summary>
    /// Represents the state of a single frame of input from client
    /// picked up from unity's input manager
    /// </summary>

    [Serializable]
    public class BaseInputFrame
    {
        public static BaseInputFrame Empty
        {
            get
            {
                return new BaseInputFrame();
            }
        }

        public uint frameNumber;
        public bool right;
        public bool down;
        public bool left;
        public bool up;
        public float horizontal;
        public float vertical;

        public bool hasInput
        {
            get
            {
                return right || left || up || down;
            }
        }

        // Equality operator overload to check 
        // if input has changed and do not care about the FrameNumber, TimeStamp or input axis

        public static bool operator == (BaseInputFrame input1, BaseInputFrame input2)
        {
            // Null & reference check
            if (ReferenceEquals(input1, input2))
            {
                return true;
            }
            else if (ReferenceEquals(input1, null) || ReferenceEquals(input2, null))
            {
                return false;
            }

            return input1.right == input2.right &&
                   input1.left == input2.left &&
                   input1.up == input2.up &&
                   input1.down == input2.down;
        }
        // Inequality operator overload (inverted)
        public static bool operator !=(BaseInputFrame input1, BaseInputFrame input2)
        {
            // Null & reference check
            if (ReferenceEquals(input1, input2))
            {
                return false;
            }
            else if (ReferenceEquals(input1, null) || ReferenceEquals(input2, null))
            {
                return true;
            }

            return input1.right != input2.right &&
                   input1.left != input2.left &&
                   input1.up != input2.up &&
                   input1.down != input2.down;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            else if (ReferenceEquals(this, null) || ReferenceEquals(obj, null))
            {
                return false;
            }

            return right == ((BaseInputFrame)obj).right &&
                   left == ((BaseInputFrame)obj).left &&
                   up == ((BaseInputFrame)obj).up &&
                   down == ((BaseInputFrame)obj).down;
        }

        public override int GetHashCode()
        {
            return right.GetHashCode() ^ left.GetHashCode() ^ up.GetHashCode() ^ 
                   down.GetHashCode() ^ horizontal.GetHashCode() ^ vertical.GetHashCode();
        }
    }
}
