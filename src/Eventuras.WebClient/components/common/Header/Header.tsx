import { Button, Flex, Heading, useColorModeValue } from "@chakra-ui/react";
import { Link, UserMenu } from "..";

import React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { useRouter } from 'next/router'

const Header = () => {
  const {
    isAuthenticated,
    user,
    loginWithRedirect,
    logout,
  } = useAuth0();
  const { pathname, query } = useRouter();

  return (
    <Flex
      as="nav"
      align="center"
      justify="space-between"
      wrap="wrap"
      padding="1.5rem"
      bg={useColorModeValue("gray.100", "gray.900")}
      color={useColorModeValue("gray.600", "gray.300")}
    >
      <Flex align="center" mr={5}>
        <Heading as="h1" size="lg" letterSpacing={"-.1rem"}>
          <Link href="/">Eventuras</Link>
        </Heading>
      </Flex>

      {!isAuthenticated && (
        <Button
          onClick={() => loginWithRedirect({ appState: { returnTo: { pathname, query } } })}
          colorScheme="teal"
          variant="outline"
        >
          Logg på
        </Button>
      )}
      {isAuthenticated && <UserMenu name={user.name} onClickLogout={() => logout({ returnTo: window.location.origin })}/>}
    </Flex>
  );
};

export default Header;
