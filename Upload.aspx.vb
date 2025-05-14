Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Request.HttpMethod = "POST" Then

        Dim cn As New System.Data.SqlClient.SqlConnection(GetConnectionString())
        cn.Open()

        If Request.Form("btnDelete") <> "" Then
            'Delete files
            If (Not Request.Form.GetValues("chkDelete") Is Nothing) Then
                For i As Integer = 0 To Request.Form.GetValues("chkDelete").Length - 1
                    Dim sFileId As String = Request.Form.GetValues("chkDelete")(i)

                    Try
                        Dim cm As New SqlCommand("delete AppFile where FileId = @FileId", cn)
                        cm.Parameters.Add("@FileId", Data.SqlDbType.Int).Value = sFileId
                        cm.ExecuteNonQuery()
                    Catch ex As Exception
                        'Ignore error
                    End Try
                Next
            End If

        Else
            Dim sJson As String = ""

            'Upload Files
            For i As Integer = 0 To Request.Files.Count - 1
                Dim oFile As System.Web.HttpPostedFile = Request.Files(i)
                Dim sFileName As String = System.IO.Path.GetFileName(oFile.FileName)

                If sFileName <> "" Then
                    Dim iFileSize As Integer = oFile.ContentLength
                    Dim oData(iFileSize) As Byte
                    Dim oStream As System.IO.Stream = oFile.InputStream
                    oStream.Read(oData, 0, iFileSize)
                    oStream.Close()

                    Dim sSql As String = "INSERT INTO AppFile _
                      (FileData, FileName, FileContentType, FileSize) " & _
                      " Values(@FileData, @FileName, @FileContentType, @FileSize); _
                      SELECT @@IDENTITY"

                    Dim cm As New SqlCommand(sSql, cn)
                    cm.Parameters.Add("@FileData", _
                           Data.SqlDbType.Binary, iFileSize).Value = oData
                    cm.Parameters.Add("@FileName", Data.SqlDbType.NVarChar).Value = sFileName
                    cm.Parameters.Add("@FileContentType", Data.SqlDbType.NVarChar).Value = _
                                   oFile.ContentType
                    cm.Parameters.Add("@FileSize", Data.SqlDbType.Int).Value = iFileSize
                    Dim sFileId As String = cm.ExecuteScalar()

                    sJson += "oUploadedFiles.push({fileId: " & sFileId & ", _
                           name: """ & sFileName & """, size: " & iFileSize & "});"
                End If
            Next

            If Request.Form("btnUpload") = "" Then
                Response.Write(sJson)
                Response.End()
            End If
        End If

        cn.Close()
    End If

    SetupDatabase()
End Sub

Private Sub SetupDatabase()
    Dim cn As New System.Data.SqlClient.SqlConnection(GetConnectionString())
    cn.Open()

    Dim cm As New SqlCommand("select count(*) from INFORMATION_SCHEMA.TABLES _
                  where TABLE_NAME = 'AppFile'", cn)
    If cm.ExecuteScalar() = "1" Then
        'Table already exists
        cn.Close()
        Exit Sub
    End If

    Dim sSql As String = System.Configuration.ConfigurationManager.AppSettings("TableCreate")
    cm = New SqlCommand(sSql, cn)
    cm.ExecuteNonQuery()
    cn.Close()
End Sub

Public Sub ShowFiles()

    Dim sSql As String = "SELECT FileId, FileName, FileSize, DateCreated from AppFile"

    Dim sSort As String = Request.QueryString("sort") & ""
    If sSort = "FileName" OrElse sSort = "FileSize" OrElse sSort = "DateCreated" Then
        sSql += " ORDER BY " & sSort
    End If

    Dim cn As New SqlConnection(GetConnectionString())
    cn.Open()
    Dim ad As SqlDataAdapter = New SqlDataAdapter(sSql, cn)
    Dim ds As Data.DataSet = New Data.DataSet
    ad.Fill(ds)
    cn.Close()
    Dim oTable As Data.DataTable = ds.Tables(0)

    If oTable.Rows.Count = 0 Then
        Exit Sub
    End If

    Response.Write("<table id='tbServer' _
           class='StatusTable' border=1 cellspacing=0 cellpadding=3>")
    Response.Write("<tr id=trHeader>")
    Response.Write("<th><a href='?sort=FileName'>File name</a></th>")
    Response.Write("<th><a href='?sort=FileSize'>Size</a></th>")
    Response.Write("<th><a href='?sort=DateCreated'>Date Modified</a></th>")
    Response.Write("<th><label><input type=checkbox name=chkDeleteAll _
              onclick='DeleteAll(this)'>Delete</label></th></tr>")

    For i As Integer = 0 To oTable.Rows.Count - 1
        Dim sFileId As String = oTable.Rows(i)("FileId")
        Dim sFileName As String = oTable.Rows(i)("FileName")
        Dim iFileSize As Integer = oTable.Rows(i)("FileSize")
        Dim dDateCreated As DateTime = oTable.Rows(i)("DateCreated")

        Dim sSize As String = FormatNumber((iFileSize / 1024), 0)
        If sSize = "0" AndAlso iFileSize > 0 Then sSize = "1"

        Response.Write("<tr>")
        Response.Write("<td><a href=""Download.aspx?id=" & sFileId & _
                     """ target='_blank'>" & sFileName + "</a></td>")
        Response.Write("<td>" & sSize & " KB</td>")
        Response.Write("<td>" & dDateCreated.ToShortDateString() & _
                     " " & dDateCreated.ToShortTimeString() & "</td>")
        Response.Write("<td><input type=checkbox name=chkDelete _
        value=""" & sFileId & """>")
        Response.Write("</tr>")
    Next

    Response.Write("</table>")
End Sub

Private Function GetConnectionString() As String
    Return System.Configuration.ConfigurationManager.AppSettings("ConnectionString")
End Function
