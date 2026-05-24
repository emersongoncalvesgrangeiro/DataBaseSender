using MySql.Data.MySqlClient;

namespace DataBaseSystem {
  public class DBConnector {
    private string connectionstring;
    public DBConnector(string stringconnection) {
      connectionstring = stringconnection;
    }
    public async Task Connect(string leadername, string teamname, string hash, int codenumber, string filename, byte[] source) {
      using MySqlConnection conn = new MySqlConnection(connectionstring);
      string query = $"INSERT INTO Assessment (leaderName, teamname, teamhash, codenumber, filename, source) " + "VALUES (@leadername, @teamname, @teamhash, @codenumber, @filename, @source);";
      try {
        await conn.OpenAsync();
        using MySqlCommand cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@leadername", leadername);
        cmd.Parameters.AddWithValue("@teamname", teamname);
        cmd.Parameters.AddWithValue("@teamhash", hash);
        cmd.Parameters.AddWithValue("@codenumber", codenumber);
        cmd.Parameters.AddWithValue("@filename", filename);
        cmd.Parameters.AddWithValue("@source", source);
        await cmd.ExecuteNonQueryAsync();
      } catch (Exception ex) {
        Console.WriteLine("An error occurred: " + ex.Message);
      }
      finally {
        conn.Close();
      }
    }
  }
}