#!/bin/bash
export LANG="en_US.UTF-8"

RootPath=$(pwd)
publish_properties=publish.properties
version_properties=version.properties
global_properties=global.properties

buildType=$5
platform=$1
subPlatform=$2
versionName=$3
versionCode=$4
IPAType=$6
package=,appname=,cdn=,plugins=,icon=,splash=
#podsType可以为Project\Workspace
podsType=Project

# __readINI [配置文件路径+名称] [节点名] [键值]
function __readINI() {
	INIFILE=$1; SECTION=$2; ITEM=$3
	#_readIni=`awk -F '=' '/\['$SECTION'\]/{a=1}a==1&&$1~/'$ITEM'/{print $2;exit}' $INIFILE`
	_readIni=`awk -F '=' "/\[${SECTION}\]/{a=1}a==1" ${INIFILE}|sed -e '1d' -e '/^$/d' -e '/^\[.*\]/,$d' -e "/^${ITEM}=.*/!d" -e "s/^${ITEM}=//"`
	echo ${_readIni}
}

function __readINI2() {
	INIFILE=$1; SECTION=$2; ITEM=$3
	_readIni=`awk -F '=' '/\['$SECTION'\]/{a=1}a==1&&$1~/'$ITEM'/{print $2;exit}' $INIFILE`
	#_readIni=`awk -F '=' "/\[${SECTION}\]/{a=1}a==1" ${INIFILE}|sed -e '1d' -e '/^$/d' -e '/^\[.*\]/,$d' -e "/^${ITEM}=.*/!d" -e "s/^${ITEM}=//"`
	echo ${_readIni}
}

# __readIniSections [配置文件路径+名称]
function __readIniSections() {
    INIFILE=$1
    val=$(awk '/\[/{printf("%s ",$1)}' ${INIFILE} | sed 's/\[//g' | sed 's/\]//g')
    echo ${val}
}

function __showPlatforms(){
	val=""
	array=($(__readIniSections ${publish_properties}))
	for var in ${array[@]}
	do
		array=(${var//-/ })  	
		if [[ ${val} =~ ${array[0]} ]]
		then
		  t="包含"
		else
		  val=${val}" "${array[0]}
		fi
	done
	echo $val
}

# __showSubPlatforms [platform]
function __showSubPlatforms(){
	platform=$1
	val=""
	array=($(__readIniSections ${publish_properties}))
	for var in ${array[@]}
	do
		array=(${var//-/ })  
		if [ "${array[0]}" == "${platform}" ]
		then
			val=${val}" "${array[1]}
		fi
	done
	echo $val
}

function __inputPlatforms(){
	__showPlatforms
	read -p "input platform:" platform
	__showSubPlatforms $platform
	read -p "input sub platform:" subPlatform
	if [ -z "$subPlatform" ];then
		subPlatform=default
	fi
}

# __getPublishProperties [键值]
function __getPublishProperties(){
	_value=( "$( __readINI ${publish_properties} ${platform}-${subPlatform} $1 )" )
	if [ -z "$_value" ]; then
		_value=( "$( __readINI ${publish_properties} ${platform}-default $1 )" )
	fi

	echo $_value
}

function __showVersion(){
	echo ------------Version------------
	if [ ! -f ${version_properties} ];then
		echo "1.0.0" > ${version_properties}
		echo "100" >>  ${version_properties}
	fi
	
	idx=0
	for line in `cat ${version_properties}`
	do
		let idx=$idx+1
		if [ $idx == 1 ];then
			versionName=$line
			echo "VersionName:"$versionName
		elif [ $idx == 2 ];then
			versionCode=$line
			echo "versionCode:"$versionCode
		fi
	done
	echo -e "\n"
}

function __modifyVersion(){
	read -p "VersionName:" versionName
	read -p "VersionCode:" versionCode
	echo $versionName > ${version_properties}
	echo $versionCode >>  ${version_properties}
}

#构建xcode工程
function __buildUnity2Xcode(){
	tempDir=$RootPath/build
	if [ -d "$tempDir" ];then
		rm -rf $tempDir
	fi
	mkdir $tempDir
	if [ ! -d "ipa" ];then
		mkdir ipa
	fi

	package=( "$( __getPublishProperties package )" )
	appname=( "$( __getPublishProperties appname )" )
	cdn=( "$( __getPublishProperties cdn )" )
	plugins=( "$( __getPublishProperties plugins )" )
	icon=( "$( __getPublishProperties icon )" )
	splash=( "$( __getPublishProperties splash )" )
	echo $package $appname $cdn $plugins $icon $splash
	
	echo -e "\n------------Xcode export,please wait------------"
	time=$(date "+%Y%m%d%H%M%S")
	logfile=./logs/$time.log
	UNITY_PATH=( "$( __readINI ${global_properties} unity unity.exe )" )
	UNITY_PROJECT_PATH=( "$( __readINI ${global_properties} unity project.dir )" )
	XCODE_OUT_PATH=( "$( __readINI ${global_properties} unity xcode.outdir )" )
	MONO_PATH=( "$( __readINI ${global_properties} mono mono.exe )" )
	echo $XCODE_OUT_PATH
	
	if [ ! -d $XCODE_OUT_PATH ];then
		mkdir $XCODE_OUT_PATH
	fi
	
	#kill unity
	#ps -ef | grep Unity | grep -v grep | awk '{print $2}' | xargs kill -9
	parameter="platform=$platform versionName=$versionName versionCode=$versionCode buildType=$buildType package=$package appName=$appname cdn=$cdn plugins=$plugins icon=$icon splash=$splash xcodeOut=$XCODE_OUT_PATH"
	#build unity
	"$UNITY_PATH" -projectPath "$UNITY_PROJECT_PATH" -executeMethod BuildiOS.Build $parameter -quit -batchmode -logFile $logfile
	#"$UNITY_PATH" -projectPath "$UNITY_PROJECT_PATH" -executeMethod BuildiOS.Build $parameter -logFile $logfile
	
	cp -r $XCODE_OUT_PATH/. $tempDir/${platform}
	#gen app XcodeSetting.json
	appConfigPath=$tempDir/XcodeSetting.json
	echo {>$appConfigPath
	echo -e	"	\"plist\":{">>$appConfigPath
	echo -e	"		\"CFBundleDisplayName\":\"$appname\",">>$appConfigPath
	echo -e	"		\"CFBundleIdentifier\":\"$package\",">>$appConfigPath
	echo -e	"		\"CFBundleShortVersionString\":\"$versionName\"">>$appConfigPath
	echo -e	"		\"CFBundleVersion\":\"$versionCode\"">>$appConfigPath
	echo -e	"	}">>$appConfigPath
	echo }>>$appConfigPath
	
	tempXcodeDir="$tempDir"/$platform
	echo -e "\n------------app XcodeSetting------------"
	#usdkConfigPath=$RootPath/sdk/usdk/module/XcodeSetting.json
	"$MONO_PATH" ./tools/XcodeSetting.exe --pbx "$tempXcodeDir" $appConfigPath
	
	echo -e "\n------------Usdk XcodeSetting------------"
	usdkConfigPath=$RootPath/sdk/usdk/module/XcodeSetting.json
	"$MONO_PATH" ./tools/XcodeSetting.exe --pbx "$tempXcodeDir" $usdkConfigPath
	
	echo -e "\n------------platform XcodeSetting------------"
	platformConfigPath=$RootPath/sdk/platforms/$platform/module/XcodeSetting.json
	"$MONO_PATH" ./tools/XcodeSetting.exe --pbx "$tempXcodeDir" $platformConfigPath
	platPodConfigPath=$RootPath/sdk/platforms/$platform/module/CocoaPods.json
	if [ -f $platPodConfigPath ];then
		"$MONO_PATH" ./tools/XcodeSetting.exe --pod "$tempXcodeDir" $platPodConfigPath ${podsType}
	fi
	
	echo -e "\n------------plugins XcodeSetting------------"
	array=(${plugins//,/ }) 
	for var in ${array[@]}
	do
		pluginConfigPath=$RootPath/sdk/plugins/$var/module/XcodeSetting.json
		"$MONO_PATH" ./tools/XcodeSetting.exe --pbx "$tempXcodeDir" $pluginConfigPath
		
		pluginPodConfigPath=$RootPath/sdk/plugins/$var/module/CocoaPods.json
		if [ -f $pluginPodConfigPath ];then
			"$MONO_PATH" ./tools/XcodeSetting.exe --pod "$tempXcodeDir" $pluginPodConfigPath ${podsType}
		fi
	done
}

#构建ipa包
function __buildIPA(){
	targetname=Unity-iPhone
	xcodeprojname=$targetname.xcodeproj
	xcodeproj=${tempXcodeDir}/$xcodeprojname
	
	UNITY_VER=2018
	if [ -d "${tempXcodeDir}/UnityFramework/" ];then
		UNITY_VER=2019
	fi

	CODE_SIGN_IDENTITY=( "$( __readINI ${global_properties} CODE_SIGN_IDENTITY $IPAType )" )
	PROVISIONING_PROFILE=( "$( __readINI ${global_properties} PROVISIONING_PROFILE $IPAType )" )
	time=$(date "+%Y%m%d%H%M%S")

	echo -e "\n------------build ipa ,please wait------------"
	ExportOptionsPlist=${tempXcodeDir}/ExportOptionsPlist.plist
	echo -e "<plist version=\"1.0\">">$ExportOptionsPlist
	echo -e	"<dict>">>$ExportOptionsPlist
	echo -e	"		<key>provisioningProfiles</key>">>$ExportOptionsPlist
	echo -e	"		<dict>">>$ExportOptionsPlist
	echo -e	"			<key>${package}</key>">>$ExportOptionsPlist
	echo -e	"			<string>$PROVISIONING_PROFILE</string>">>$ExportOptionsPlist
	echo -e	"		</dict>">>$ExportOptionsPlist
	echo -e	"		<key>method</key>">>$ExportOptionsPlist
	echo -e	"		<string>$IPAType</string>">>$ExportOptionsPlist
	echo -e	"		<key>uploadBitcode</key>">>$ExportOptionsPlist
	echo -e	"		<false/>">>$ExportOptionsPlist
	echo -e	"</dict>">>$ExportOptionsPlist
	echo -e	"</plist>">>$ExportOptionsPlist

	xcodebuild clean -project $xcodeproj -configuration Release -alltargets
	if [ ${UNITY_VER} == 2019 ];then
		xcodebuild archive -project $xcodeproj -scheme $targetname -configuration Release -archivePath build/$targetname-$IPAType.xcarchive clean archive CONFIGURATION_BUILD_DIR=$tempXcodeDir/configuration CODE_SIGN_IDENTITY_APP="$CODE_SIGN_IDENTITY" PROVISIONING_PROFILE_APP=$PROVISIONING_PROFILE
	else
		xcodebuild archive -project $xcodeproj -scheme $targetname -configuration Release -archivePath build/$targetname-$IPAType.xcarchive clean archive CONFIGURATION_BUILD_DIR=$tempXcodeDir/configuration CODE_SIGN_IDENTITY_="$CODE_SIGN_IDENTITY" PROVISIONING_PROFILE_=$PROVISIONING_PROFILE
	fi

	xcodebuild -exportArchive -archivePath build/$targetname-$IPAType.xcarchive -exportOptionsPlist $ExportOptionsPlist -exportPath ipa
}

function __main(){
	chmod +x global.properties
	dos2unix global.properties
	chmod +x publish.properties
	dos2unix publish.properties
	chmod +x version.properties
	dos2unix version.properties
	
	echo $versionName > ${version_properties}
	echo $versionCode >>  ${version_properties}
	
	__showVersion
	__buildUnity2Xcode
	__buildIPA
}

__main