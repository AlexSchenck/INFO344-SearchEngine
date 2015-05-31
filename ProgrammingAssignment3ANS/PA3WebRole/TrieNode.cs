using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PA3WebRole
{
    public class TrieNode
    {
        public char letter; //Letter node represents
        private Dictionary<char, TrieNode> childrenNodes; //Dictionary representation of trie child nodes
        public bool isTitle; //True if the node represents the end of a title despite having child nodes

        //Constructor for root node, contains no letter information
        public TrieNode()
        {
            childrenNodes = new Dictionary<char, TrieNode>();
            isTitle = false;
        }

        //Constructor for a non-root node that contains information for a letter or space
        public TrieNode(char letter)
        {
            this.letter = letter;
            childrenNodes = new Dictionary<char, TrieNode>();
            isTitle = false;
        }

        //Adds child node of given character to this node
        public void addChild(char letter)
        {
            childrenNodes.Add(letter, new TrieNode(letter));
        }

        //Returns true if this node has a child node with the given letter
        public bool hasChild(char letter)
        {
            return childrenNodes.ContainsKey(letter);
        }

        //Returns child node of given letter
        public TrieNode getChild(char letter)
        {
            if (!hasChild(letter))
            {
                return null;
            }
            else
            {
                return childrenNodes[letter];
            }
        }

        //Returns dictionary of all children nodes of this node
        public Dictionary<char, TrieNode> getChildrenNodes()
        {
            return childrenNodes;
        }
    }
}