//
//  NPWhatsAppShareComposer.h
//  Native Plugins
//
//  Created by Ashwin kumar on 22/01/19.
//  Copyright (c) 2019 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <Social/Social.h>
#import "NPSharingServicesDataTypes.h"

@interface NPWhatsAppShareComposer : NSObject<UIDocumentInteractionControllerDelegate, NPSocialShareComposer>

// static methods
+ (bool)IsServiceAvailable;

@end
