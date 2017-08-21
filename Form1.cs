using Common;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormFTPFile_Compal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string direc_pdf = ConfigurationManager.AppSettings["filedir"];//文件服务器存放文件的一级目录

        string compal_ftp_username = ConfigurationManager.AppSettings["compalftpusername"];
        string compal_ftp_psd = ConfigurationManager.AppSettings["compalftppassword"];
        System.Uri compal_ftp_uri = new Uri("ftp://" + ConfigurationManager.AppSettings["compalftpip"] + ":21");
        FtpHelper ftp = null;
        IDatabase db = SeRedis.redis.GetDatabase();

        private void Form1_Load(object sender, EventArgs e)
        {
            ftp = new FtpHelper(compal_ftp_uri, compal_ftp_username, compal_ftp_psd);
            if (ConfigurationManager.AppSettings["AutoRun"].ToString().Trim() == "Y")
            {
                btn_AutoRun_Click(sender, e);
                this.Close();
            }
        }

        private void btn_AutoRun_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            try
            {
                btn_AutoRun.Enabled = false;

                string destination = DateTime.Now.ToString("yyyy-MM-dd");
                List<FileStruct> fis = ftp.GetFileAndDirectoryList(@"\");
                foreach (FileStruct fs in fis)
                {
                    int seconds = Convert.ToInt32((DateTime.Now - fs.UpdateTime.Value).TotalSeconds);
                    #region 处理文件开始
                    if (!fs.IsDirectory && fs.Size > 0 && seconds > 10)//有时候文件还在生成中，故加上时间范围限制
                    {
                        //提取合同协议号 如果无_，则直接将文件主名称作为合同协议号,如果有,则截取
                        int start = fs.Name.IndexOf("_");
                        string contractno = string.Empty;
                        if (start >= 0)
                        {
                            contractno = fs.Name.Substring(0, start);
                        }
                        else
                        {
                            start = fs.Name.IndexOf("-");//有些文件比较特殊是中杠
                            if (start >= 0)
                            {
                                contractno = fs.Name.Substring(0, start);
                            }
                            else
                            {
                                start = fs.Name.IndexOf(".");
                                contractno = fs.Name.Substring(0, start);
                            }
                        }
                        bool content = update_entorder(fs, destination, contractno);
                        //如果数据库信息插入或者更新成功
                        if (content)
                        {
                            if (!Directory.Exists(direc_pdf + destination))
                            {
                                Directory.CreateDirectory(direc_pdf + destination);
                            }
                            bool result = false;
                            if (fs.Name.IndexOf(".txt") > 0 || fs.Name.IndexOf(".TXT") > 0)
                            {
                                string[] split = fs.Name.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                result = ftp.DownloadFile(@"\" + fs.Name, direc_pdf + destination + @"\" + split[0] + "_0." + split[1]);
                                if (result) //TXT文件在下载成功的情况下
                                {
                                    try
                                    {
                                        StreamReader sr = new StreamReader(direc_pdf + destination + @"\" + split[0] + "_0." + split[1], Encoding.GetEncoding("BIG5"));
                                        String line;
                                        FileStream fs2 = new FileStream(direc_pdf + destination + @"\" + fs.Name, FileMode.Create);
                                        while ((line = sr.ReadLine()) != null)
                                        {
                                            byte[] dst = Encoding.UTF8.GetBytes(line);
                                            fs2.Write(dst, 0, dst.Length);
                                            fs2.WriteByte(13);
                                            fs2.WriteByte(10);
                                        }
                                        fs2.Flush();  //清空缓冲区、关闭流
                                        fs2.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        lbl_msg.Text = "big5_to_utf8_" + ex.Message;
                                        break;//add by panhuaguo 20170118
                                    }
                                }
                                //else
                                //{
                                //    break;//如果txt文件下载失败
                                //}
                            }
                            else
                            {
                                result = ftp.DownloadFile(@"\" + fs.Name, direc_pdf + destination + @"\" + fs.Name);
                            }
                            if (result)//下载成功的情况下
                            {
                                ftp.MoveFile(@"\" + fs.Name, @"\backup\" + fs.Name);
                            }
                            //else
                            //{
                            //    break;//如果f文件下载失败
                            //}
                        }
                        //else
                        //{
                        //    break;//数据库写入失败
                        //}
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                lbl_msg.Text = "out_" + ex.Message;
            }
            btn_AutoRun.Enabled = true;
        }

        private bool update_entorder(FileStruct fs, string directory, string contractno)
        {
            string sql = string.Empty;
            bool content = false;
            #region
            try
            {
                string enterprisecode = string.Empty;
                string enterprisename = string.Empty;

                string prefix = fs.Name.Substring(0, 3);
                if (fs.Name.IndexOf(".txt") > 0 || fs.Name.IndexOf(".TXT") > 0)
                {
                    prefix = fs.Name.Substring(4, 3);
                }

                string entid = string.Empty;
                if (prefix == "E1P" || prefix == "E1B" || prefix == "IMP" || prefix == "IMB")
                {
                    enterprisecode = "3223640003";//海关10位编码  空运出口
                    enterprisename = "仁宝电子科技(昆山)有限公司";
                }
                if (prefix == "E1W" || prefix == "E2W" || prefix == "E1D" || prefix == "E2D" || prefix == "E7D" || prefix == "IMW" || prefix == "IMD" || prefix == "LMW" || prefix == "LMD" || prefix == "IAD" || prefix == "IEW" || prefix == "IED" || prefix == "E7W" || prefix == "LDD" || prefix == "LGW" || prefix == "LGD" || prefix == "LDW")
                {
                    enterprisecode = "3223640047";
                    enterprisename = "仁宝信息技术(昆山)有限公司";
                }
                if (prefix == "E1C" || prefix == "E1Q" || prefix == "E1O" || prefix == "IMQ" || prefix == "IMC" || prefix == "E2Q" || prefix == "E2C" || prefix == "LGC")
                {
                    enterprisecode = "3223640038";
                    enterprisename = "仁宝资讯工业(昆山)有限公司";
                }
                if (prefix == "IVS" || prefix == "EAS")
                {
                    enterprisecode = "3223660037";
                    enterprisename = "昆山柏泰电子技术服务有限公司";
                }
                //code是企业编号 仁宝格式 E1Q1603927_sheet.txt 
                int start = fs.Name.LastIndexOf("_");
                int end = fs.Name.LastIndexOf(".");
                string suffix = fs.Name.Substring(end + 1, 3).ToUpper();//文件扩展名
                string filetype = fs.Name.Substring(start + 1, end - start - 1).ToUpper();
                int filetypeid = 0;
                switch (filetype)
                {
                    case "CONTRACT":
                        filetypeid = 50;
                        break;
                    case "INVOICE":
                        filetypeid = 51;
                        break;
                    case "PACKING":
                        filetypeid = 52;
                        break;
                    case "SHEET":
                        filetypeid = 44;
                        break;
                    default:
                        filetypeid = 50;
                        break;
                }
                sql = "select * from ent_order where code='" + contractno + "'";
                DataTable dt_ent = DBMgr.GetDataTable(sql);
                if (dt_ent.Rows.Count == 0)
                {
                    sql = "select ENT_ORDER_ID.Nextval from dual";
                    entid = DBMgr.GetDataTable(sql).Rows[0][0] + "";
                    sql = @"insert into ent_order(ID,CODE,CREATETIME,SUBMITTIME,UNITCODE,ENTERPRISECODE,ENTERPRISENAME,FILEDECLAREUNITCODE,FILEDECLAREUNITNAME,
                            FILERECEVIEUNITCODE,FILERECEVIEUNITNAME,TEMPLATENAME,CUSTOMDISTRICTCODE,CUSTOMDISTRICTNAME) VALUES
                            ('{3}','{0}',sysdate,sysdate,(select fun_AutoQYBH(sysdate) from dual),'{1}','{2}','{4}','{5}','{6}','{7}','COMPAL01','2369','昆山综保')";
                    sql = string.Format(sql, contractno, enterprisecode, enterprisename, entid, "3223980002", "江苏飞力达国际物流股份有限公司", "3223980002", "江苏飞力达国际物流股份有限公司");
                    DBMgr.ExecuteNonQuery(sql);
                }
                else
                {
                    entid = dt_ent.Rows[0]["ID"] + "";
                }
                //写入随附文件表 
                sql = @"select * from list_attachment where originalname='" + fs.Name + "' and entid='" + entid + "'";
                DataTable dt_att = DBMgr.GetDataTable(sql);//因为客户有可能会重复传文件,此是表记录不需要变化，替换文件即可
                if (dt_att.Rows.Count > 0)
                {
                    sql = "delete from list_attachment where id='" + dt_att.Rows[0]["ID"] + "'";
                    DBMgr.ExecuteNonQuery(sql);
                }
                //dt_att = DBMgr.GetDataTable("select LIST_ATTACHMENT_ID.Nextval ATTACHMENTID from dual");
                sql = @"insert into list_attachment(ID,FILENAME,ORIGINALNAME,UPLOADTIME,FILETYPE,SIZES,ENTID,FILESUFFIX,UPLOADUSERID,CUSTOMERCODE,isupload) values(
                   LIST_ATTACHMENT_ID.Nextval,'{0}','{1}',sysdate,'{2}','{3}','{4}','{5}','404','{6}','1')";
                sql = string.Format(sql, "/" + directory + "/" + fs.Name, fs.Name, filetypeid, fs.Size, entid, suffix, enterprisecode);
                int result = DBMgr.ExecuteNonQuery(sql);
                if (result > 0 && fs.Name.IndexOf(".txt") > 0)
                {
                    db.ListRightPush("compal_sheet_topdf_queen", "{ENTID:'" + entid + "',FILENAME:" + "'/" + directory + "/" + fs.Name + "'}");//保存随附文件ID到队列
                }
                content = true;
            }
            #endregion
            catch (Exception ex)
            {
                lbl_msg.Text = "database_" + ex.Message;
                content = false;
            }
            return content;
        }
    }
}
