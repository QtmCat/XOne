using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QtmCatFramework
{
	public class StateMachine
	{
		private Dictionary<int, State> stateDic = new Dictionary<int, State>();
		private State                  curState;
		private State                  preStae;


		public void AddState(State state)
		{
			// stateDic.Add(state.id, state);
		}



		public class State
		{
			private int    id;
			private object userData;
			private Action OnEnter;
			private Action OnExit ;
			private Action Update ;

			public State SetId(int id)
			{
				this.id = id;
				return this;
			}

			public State SetUserData(object userData)
			{
				this.userData = userData;
				return this;
			}

			public State SetOnEnter(Action OnEnter)
			{
				this.OnExit = OnEnter;
				return this;
			}

			public State SetOnExit(Action OnExit)
			{
				this.OnExit = OnExit;
				return this;
			}

			public State SetUpdate(Action Update)
			{
				this.Update = Update;
				return this;
			}
		}
	}
}

