#coding:utf-8 

import os

if __name__=='__main__':
    root = os.getcwd()
    FileList = os.listdir(root)
    FileList = [root+'\\'+x for x in FileList if os.path.isdir(root+'\\'+x)]
    for item in FileList:
        if os.path.exists(item+'\\bin'):
		os.rmdir(item+'\\bin')
        if os.path.exists(item+'\\obj'):
                os.rmdir(item+'\\obj')
	