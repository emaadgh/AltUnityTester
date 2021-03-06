import os
import unittest
import sys
from appium import webdriver
from altunityrunner import AltrunUnityDriver

PATH = lambda p: os.path.abspath(
    os.path.join(os.path.dirname(__file__), p)
)

class SampleAppiumTest(unittest.TestCase):
    altdriver = None
    platform = "android" # set to `ios` or `android` to change platform

    @classmethod
    def setUpClass(cls):
        cls.desired_caps = {}
        if (cls.platform == "android"):
            cls.setup_android()
        else:
            cls.setup_ios()
        cls.driver = webdriver.Remote('http://localhost:4723/wd/hub', cls.desired_caps)
        cls.altdriver = AltrunUnityDriver(cls.driver, cls.platform)

    @classmethod
    def tearDownClass(cls):
        cls.altdriver.stop()
        cls.driver.quit()

    @classmethod
    def setup_android(cls):
        cls.desired_caps['platformName'] = 'Android'
        cls.desired_caps['deviceName'] = 'device'
        cls.desired_caps['app'] = PATH('../../../sampleGame.apk')

    @classmethod
    def setup_ios(cls):
        cls.desired_caps['platformName'] = 'iOS'
        cls.desired_caps['deviceName'] = 'iPhone5'
        cls.desired_caps['automationName'] = 'XCUITest'
        cls.desired_caps['app'] = PATH('../../../sampleGame.ipa')

    def test_find_element_and_tap(self):
        # tap UIButton to make capsule jump
        self.altdriver.find_element('UIButton').mobile_tap()
        capsule_info = self.altdriver.wait_for_element_with_text('CapsuleInfo', 'UIButton clicked to jump capsule!')
        assert capsule_info.get_text() == capsule_info.get_text()
        
        # tap capsule to make it jump
        self.altdriver.find_element('Capsule').mobile_tap()
        self.altdriver.wait_for_element_with_text('CapsuleInfo', 'Capsule was clicked to jump!')


if __name__ == '__main__':
    suite = unittest.TestLoader().loadTestsFromTestCase(SampleAppiumTest)
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    sys.exit(not result.wasSuccessful())