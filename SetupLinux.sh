echo ""
echo "****************************"
echo "* Installing .NET Core 3.1 *"
echo "****************************"
echo ""

wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-3.1


echo ""
echo "***********************************"
echo "* Installing EntityFramework Core *"
echo "***********************************"
echo ""

sudo dotnet tool install --global dotnet-ef


echo ""
echo "****************************"
echo "* Installing Node.js, npm & yarn *"
echo "****************************"
echo ""

sudo apt update
sudo apt install nodejs
sudo apt install npm
sudo npm install yarn -g


echo ""
echo "*************************"
echo "* Installing TypeScript *"
echo "*************************"
echo ""

sudo npm install -g typescript


echo ""
echo "***************************"
echo "* Installing MySql Server *"
echo "***************************"
echo ""

sudo apt update
sudo apt install mysql-server

echo ""
echo "*****************************************************************"
echo "* To login to mysql, run: sudo mysql -u root                    *"
echo "* To add security to mysql, run: sudo mysql_secure_installation *"
echo "*****************************************************************"
echo ""





#Commands that might be useful for fixing mysql problems::

#Installer with more options, including security
#sudo mysql_secure_installation

#Remove everything mysql, use carefully
#sudo apt-get remove --purge mysql*

#Shutdown mysql daemon
#sudo pkill mysqld

#Create a directory that mysql needs to run, then make mysql to the owner of that directory
#sudo mkdir -p /var/run/mysqld
#sudo chown mysql:mysql /var/run/mysqld

#Run mysql daemon, in safe mode
#sudo mysqld_safe --skip-grant-table &
