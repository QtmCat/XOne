using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QtmCatFramework
{
	public class StateMachine : MonoBehaviour
	{
		private Dictionary<int, State> stateDic = new Dictionary<int, State>();

		public State curState 
		{
			private set;
			get;
		}


		public State preState 
		{
			private set;
			get;
		}

		public Enum GetCurStateId()
		{
			return this.curState != null ? this.curState.id : (Enum) Enum.ToObject(typeof(Enum), -1);
		}


		public Enum GetPreStateId()
		{
			return this.preState != null ? this.preState.id : (Enum) Enum.ToObject(typeof(Enum), -1);
		}


		public State CreateState(Enum stateId)
		{
			State state        = new State();
			state.id           = stateId;
			state.stateMachine = this;
			this.stateDic.Add(Convert.ToInt32(stateId), state);

			return state;
		}

		public State SetState(Enum stateId)
		{
			if (this.curState != null)
			{
				if (this.curState.OnExit != null)
				{
					this.curState.OnExit();
				}
			}

			this.preState = this.curState;
			this.curState = this.stateDic[Convert.ToInt32(stateId)];

			if (this.curState.OnEnter != null)
			{
				this.curState.OnEnter();
			}

			return this.curState;
		}


        public virtual void Update ()
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
			public Enum         id;
			public object       userData;
			public StateMachine stateMachine;
			public Action       OnEnter;
			public Action       OnExit ;
			public Action       Update ;

			public State SetId(Enum id)
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

