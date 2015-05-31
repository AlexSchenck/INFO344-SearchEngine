using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PA3WebRole
{
    public class Trie
    {
        private TrieNode root;

        //Constructs empty trie tree
        public Trie()
        {
            root = new TrieNode();
        }

        //Adds given title to the trie
        public void addTitle(String title)
        {
            addTitle(title.ToLower(), root);
        }

        //Recursive add helper method
        private void addTitle(String title, TrieNode node)
        {
            //Base case -- add complete
            if (title.Length == 0)
            {
                node.isTitle = true;
                return;
            }

            //Recursive case, traverses further down trie
            //If this node does not have a child of given letter, creates it
            char letter = title[0];
            if (!node.hasChild(letter))
            {
                node.addChild(letter);
            }

            addTitle(title.Substring(1), node.getChild(letter));
        }

        //Returns list of 10 query suggestions based on given prefix
        public List<String> searchPrefix(String prefix)
        {
            TrieNode start = findStartNode(prefix, root);

            if (start == null)
                return null;
            else
                return searchPrefix(prefix.ToLower(), new List<String>(), start);
        }

        //Recursive search helper method
        private List<String> searchPrefix(String prefix, List<String> results, TrieNode node)
        {
            //Base case: 10 results retrieved
            if (results.Count == 10)
            {
                return results;
            }
            else
            {
                //Adds node to suggestions if it is a title
                if (node.isTitle)
                    results.Add(prefix);

                Dictionary<char, TrieNode> children = node.getChildrenNodes();

                if (children.Count != 0)
                {
                    //Recursive case: finds more results
                    foreach (KeyValuePair<char, TrieNode> entry in children)
                    {
                        results = searchPrefix(prefix + entry.Key, results, entry.Value);
                    }
                }

                return results;
            }
        }

        //Finds start point of search
        //Returns null if no such point exists
        private TrieNode findStartNode(String prefix, TrieNode node)
        {
            TrieNode next = node.getChild(prefix[0]);

            if (next == null)
                return null;
            else if (prefix.Length == 1)
                return next;
            else
                return findStartNode(prefix.Substring(1), next);
        }
    }
}