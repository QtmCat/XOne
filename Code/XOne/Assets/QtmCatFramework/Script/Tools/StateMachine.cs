using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QtmCatFramework
{
	public class StateMachine : MonoBehaviour
	{
		private Dictionary<int, State> stateDic = new Dictionary<int, State>();
		public  State                  curState;
		public  State                  preState;

		public State CreateState(int stateId)
		{
			State state        = new State();
			state.id           = stateId;
			state.stateMachine = this;
			this.stateDic.Add(stateId, state);

			return state;
		}

		public State SetState(int stateId)
		{
			if (this.curState != null)
			{
				if (this.curState.OnExit != null)
				{
					this.curState.OnExit();
				}
			}

			this.preState = this.curState;
			this.curState = this.stateDic[stateId];

			if (this.curState.OnEnter != null)
			{
				this.curState.OnEnter();
			}

			return this.curState;
		}


		void Update()
		{
			if (this.curState != null)
			{
				if (this.curState.Update != null)
				{
					this.curState.Update();
				}
			}
		}


		public class State
		{
			public int          id;
			public object       userData;
			public StateMachine stateMachine;
			public Action       OnEnter;
			public Action       OnExit ;
			public Action       Update ;

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

