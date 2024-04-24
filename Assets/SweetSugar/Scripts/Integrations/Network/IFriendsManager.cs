using System;
using System.Collections.Generic;

namespace SweetSugar.Scripts.Integrations.Network
{
	public interface IFriendsManager {
		void GetFriends (Action<Dictionary<string,string>> Callback) ;

		void PlaceFriendsPositionsOnMap (Action<Dictionary<string,int>> Callback);


		void Logout ();
	}
}


