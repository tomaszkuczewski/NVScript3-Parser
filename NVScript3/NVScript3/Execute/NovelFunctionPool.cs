using NVScript3.NVScript3.Exceptions;
using System;
using System.Collections.Generic;

namespace NVScript3.NVScript3.Execute
{
    public class NovelFunctionPool
    {
        private List<MulticastDelegate> mDelegates;

        public NovelFunctionPool()
        {
            mDelegates = new List<MulticastDelegate>();
        }

        public void AddFunction(MulticastDelegate callback)
        {
            mDelegates.Add(callback);
        }

        public int ContainsFunction(string name, int argsCount)
        {
            for (int i = 0; i < mDelegates.Count; i++)
            {
                var method = mDelegates[i];

                //If the right delegate was found
                if (method.Method.Name.Equals(name))
                {
                    //Check for argument count
                    if (method.Method.GetParameters().Length == argsCount)
                        return i;
                }
            }

            return -1;
        }

        public int ContainsFunction(string name)
        {
            for (int i = 0; i < mDelegates.Count; i++)
                if (mDelegates[i].Method.Name.Equals(name))
                    return i;

            return -1;
        }

        public object Call(int index, params object[] args)
        {
            try
            {
                return mDelegates[index].DynamicInvoke(args);
            }
            catch(MemberAccessException)
            {
                throw new NovelException("Member access exception for function: " + mDelegates[index].Method.Name, "", 0);
            }
            catch(ArgumentException)
            {
                throw new NovelException("Invalid arguments for function : " + mDelegates[index].Method.Name, "", 0);
            }
            catch(Exception)
            {
                throw new NovelException("Unknown exception (or wrong number of function arguments) was cought in function : " + mDelegates[index].Method.Name, "", 0);

            }
        }
    }
}
