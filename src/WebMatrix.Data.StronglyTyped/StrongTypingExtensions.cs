#region Licence

// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.

#endregion

namespace WebMatrix.Data.StronglyTyped {
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public static class StrongTypingExtensions {
		public static TextWriter Log = TextWriter.Null;

		public static IEnumerable<T> Query<T>(this Database db, string commandText, params object[] args) {
			var queryResults = db.Query(commandText, args);
			var mapper = Mapper<T>.Create();
			return (from DynamicRecord record in queryResults select mapper.Map(record)).ToList();
		}

		public static IEnumerable<T> FindAll<T>(this Database db) {
			var mapper = Mapper<T>.Create();
			var query = string.Format("select * from {0}", mapper.TableName);
			return db.Query<T>(query);
		}

		public static T FindById<T>(this Database db, object id) {
			var mapper = Mapper<T>.Create();
			var idCol = mapper.GetIdColumn();
			var query = string.Format("select * from {0} where {1} = @0", mapper.TableName, idCol.PropertyName);
			return db.Query<T>(query, id).SingleOrDefault();
		}

		public static void Insert<T>(this Database db, T toInsert) {
			var mapper = Mapper<T>.Create();
			var results = mapper.MapToInsert(toInsert);
			var sql = results.Item1;
			var parameters = results.Item2;

			Log.WriteLine(sql);

			db.Execute(sql, parameters);

			//TODO: Don't assume ID will always be an int.

			var id = (int)db.GetLastInsertId();
			var idColumn = mapper.GetIdColumn();
			idColumn.SetValue(toInsert, id);
		}

		public static void Update<T>(this Database db, T toUpdate) {
			var mapper = Mapper<T>.Create();
			var results = mapper.MapToUpdate(toUpdate);
			var sql = results.Item1;
			var parameters = results.Item2;

			Log.WriteLine(sql);

			db.Execute(sql, parameters);
		}
	}
}